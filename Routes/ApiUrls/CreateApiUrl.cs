using System.Text.Json;
using System.Transactions;
using App.Constants;
using App.Extensions;
using App.Models;
using App.Repositories;
using App.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace App.Routes.ApiUrls;


public static class CreateApiUrlRoute
{

    public static async Task<object> Do(
        [FromServices] IApiUrlRepository repoApiUrl,
        [FromServices] IApiUrlPermissionRepository repoApiUrlPermission,
        HttpContext context,
        [FromBody] ReqCreateApiUrl request,
        [FromServices] IUserPermissionRepository repoUserPermission,
        [FromServices] IPgLockRepository repoPgLock,
        CancellationToken cancellationToken
    )
    {
        var lockKey = $"{nameof(CreateApiUrlRoute)}-{JsonSerializer.Serialize(request)}";
        try
        {
            var successGetLock = await repoPgLock.GetLock(lockKey, "", TimeSpan.FromSeconds(15), cancellationToken);
            if (!successGetLock)
                throw new UnhandledException(code: ErrorConstants.FAILED_GET_LOCK, $"Failed to get lock {nameof(CreateApiUrlRoute)}");

            var dt = DateTime.UtcNow;
            var logId = context.Items[LogConstants.LOG]?.ToString();

            var validator = new ReqAuthSignInValidator().Validate(request);
            if (!validator.IsValid)
                throw new BadRequestException(validator.Errors);

            var resData = new ResCreateApiUrlData();
            resData.ApiUrlPermissions = [];

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var paramCheckApiUrl = new SearchApiUrlsParams()
            {
                Url = request.Url,
                Method = request.Method
            };
            var data = await repoApiUrl.Search(paramCheckApiUrl, cancellationToken);
            if (!data.IsNullOrEmpty())
                throw new ConflictException(code: ErrorConstants.API_URL_CREATE_API_URL_FAILED_CONFLICT, "ApiUrl already exists");
            var param = new ApiUrl
            {
                Method = request.Method,
                Url = request.Url,
                CreatedAt = dt,
                UpdatedAt = dt
            };
            var apiUrl = await repoApiUrl.Create(param, cancellationToken);
            foreach (var p in request.ApiUrlPermissions)
            {
                var paramSearchUserPermission = new SearchUserPermissionsParams()
                {
                    Id = p.UserPermissionId
                };
                var dataUserPermission = await repoUserPermission.Search(paramSearchUserPermission, cancellationToken);
                var userPermission = dataUserPermission.FirstOrDefault();
                if (userPermission == null)
                {
                    var paramSearchUserPermission2 = new SearchUserPermissionsParams()
                    {
                        Name = p.UserPermissionName
                    };
                    var dataUserPermission2 = await repoUserPermission.Search(paramSearchUserPermission, cancellationToken);
                    userPermission = dataUserPermission2.FirstOrDefault();

                    if (userPermission == null)
                    {
                        var paramCreateUserPermission = new UserPermission
                        {
                            Name = p.UserPermissionName,
                            CreatedAt = dt,
                            UpdatedAt = dt
                        };
                        userPermission = await repoUserPermission.Create(paramCreateUserPermission, cancellationToken);
                    }
                }

                var paramCreateApiUrlPermission = new ApiUrlPermission
                {
                    ApiUrlId = apiUrl!.Id,
                    UserPermissionId = userPermission!.Id,
                    UserPermissionName = userPermission!.Name,
                    CreatedAt = dt,
                    UpdatedAt = dt
                };
                var apiUrlPermissions = await repoApiUrlPermission.Create(paramCreateApiUrlPermission, cancellationToken);
                resData.ApiUrlPermissions.Add(new ResCreateApiUrlDataApiUrlPermission()
                {
                    Id = apiUrlPermissions!.Id,
                    ApiUrlId = apiUrlPermissions.ApiUrlId,
                    UserPermissionId = apiUrlPermissions.UserPermissionId,
                    UserPermissionName = apiUrlPermissions?.UserPermissionName ?? string.Empty,
                });
            }

            resData.Id = apiUrl!.Id;
            resData.Url = apiUrl.Url;
            resData.Method = apiUrl.Method;
            resData.CreatedAt = apiUrl.CreatedAt.ToDefaultString();
            resData.UpdatedAt = apiUrl.UpdatedAt.ToDefaultString();
            scope.Complete();

            return new Response<ResCreateApiUrlData>()
            {
                Data = resData,
                Log = logId!,
                Message = BasicConstants.SUCCCESS
            };
        }
        finally
        {
            await repoPgLock.ReleaseLock(lockKey, "", cancellationToken);
        }

    }

    public class ResCreateApiUrlData
    {
        public string? Id { get; set; }
        public string? Url { get; set; }
        public string? Method { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
        public List<ResCreateApiUrlDataApiUrlPermission>? ApiUrlPermissions { get; set; }

    }

    public class ResCreateApiUrlDataApiUrlPermission
    {
        public string? Id { get; set; }
        public string? ApiUrlId { get; set; }
        public string? UserPermissionId { get; set; }
        public string? UserPermissionName { get; set; }
    }


    public class ReqCreateApiUrl
    {
        public required string Url { get; set; }
        public required string Method { get; set; }
        public required ReqApiUrlPermission[] ApiUrlPermissions { get; set; }
    }

    public class ReqApiUrlPermission
    {
        public required string UserPermissionId { get; set; }
        public required string UserPermissionName { get; set; }
    }

    public class ReqAuthSignInValidator : AbstractValidator<ReqCreateApiUrl>
    {
        public ReqAuthSignInValidator()
        {
            RuleFor(i => i.Url)
                .NotEmpty()
                .WithMessage("Url is required");

            RuleFor(i => i.Method)
                .NotEmpty()
                .WithMessage("Method is required")
                .Matches("^(get|post|put|delete|patch)$")
                .WithMessage("Method must be one of get, post, put, delete, patch");


            RuleForEach(i => i.ApiUrlPermissions)
             .NotEmpty()
             .WithMessage("At least one ApiUrlPermission is required")
             .ChildRules(apiUrlPermission =>
             {
                 apiUrlPermission.RuleFor(p => p.UserPermissionId).NotEmpty().WithMessage("UserPermissionId is required");
                 apiUrlPermission.RuleFor(p => p.UserPermissionName).NotEmpty().WithMessage("UserPermissionName is required");
             });
        }
    }



}