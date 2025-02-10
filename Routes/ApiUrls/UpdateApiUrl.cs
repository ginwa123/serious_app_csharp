

using System.Text.Json;
using System.Transactions;
using App.Constants;
using App.Models;
using App.Repositories;
using App.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.ApiUrls;


public static class UpdateApiUrlRoute
{

    public static async Task<Response<object>> Do(
        HttpContext context,
        [FromServices] IPgLockRepository repoPgLock,
        [FromServices] IApiUrlRepository repoApiUrl,
        [FromServices] IApiUrlPermissionRepository repoApiUrlPermission,
        [FromServices] IUserPermissionRepository repoUserPermission,
        [FromBody] ReqUpdateApiUrlRoute request,
        [FromRoute] string id,
        CancellationToken cancellationToken
    )
    {
        var lockKey = $"{nameof(UpdateApiUrlRoute)}-{JsonSerializer.Serialize(request)}";

        try
        {
            var successGetLock = await repoPgLock.GetLock(lockKey, "", TimeSpan.FromSeconds(15), cancellationToken);
            if (!successGetLock)
                throw new UnhandledException(code: ErrorConstants.FAILED_GET_LOCK, $"Failed to get lock {nameof(UpdateApiUrlRoute)}");

            var dt = DateTime.UtcNow;
            var logId = context.Items[LogConstants.LOG]?.ToString();

            var validator = new ReqUpdateApiUrlRouteValidator().Validate(request);
            if (!validator.IsValid)
                throw new BadRequestException(validator.Errors);

            var paramSearch = new SearchApiUrlsParams { Id = id };
            var data = await repoApiUrl.Search(paramSearch, cancellationToken);
            if (data == null)
                throw new NotFoundException(code: ErrorConstants.API_URL_UPDATE_API_URL_ERROR_NOT_FOUND, "ApiUrl not found");

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var paramUpdate = new ApiUrl
            {
                Id = id,
                Url = request!.Url,
                Method = request!.Method,
                UpdatedAt = dt
            };
            await repoApiUrl.Update(paramUpdate, cancellationToken);

            var paramSearchApiUrlPermission = new SearchApiUrlPermissionsParams { ApiUrlId = id };
            var dataApiUrlPermissions = await repoApiUrlPermission.SearchApiUrlPermissions(paramSearchApiUrlPermission, cancellationToken);

            foreach (var item in dataApiUrlPermissions) // delete first
            {
                var apiUrlPermissionExist = request?.ApiUrlPermissions?.FirstOrDefault(reqApiUrlPer => reqApiUrlPer.UserPermissionId == item.UserPermissionId);
                if (apiUrlPermissionExist == null)
                {
                    await repoApiUrlPermission.Delete(item.Id, cancellationToken);
                }
            }

            // select again for the new updated
            dataApiUrlPermissions = await repoApiUrlPermission.SearchApiUrlPermissions(paramSearchApiUrlPermission, cancellationToken);
            foreach (var item in request?.ApiUrlPermissions ?? []) // check the exisitng and insert the userPerimssion
            {
                var b = dataApiUrlPermissions.FirstOrDefault(existingApiUrlPerm => existingApiUrlPerm.Id == item.Id || existingApiUrlPerm.UserPermissionId == item.UserPermissionId);
                if (b == null)
                {
                    var paramCheckUserPermission = new SearchUserPermissionsParams { Id = item.UserPermissionId };
                    var dataUserPermission = await repoUserPermission.Search(paramCheckUserPermission, cancellationToken);
                    var userPermission = dataUserPermission.FirstOrDefault();
                    if (userPermission == null)
                    {
                        var paramCheckUserPermission2 = new SearchUserPermissionsParams { Name = item.UserPermissionName };
                        var dataUserPermission2 = await repoUserPermission.Search(paramCheckUserPermission2, cancellationToken);
                        userPermission = dataUserPermission2.FirstOrDefault();

                        if (userPermission == null)
                        {
                            var paramCreateUserPermission = new UserPermission
                            {
                                Name = item.UserPermissionName,
                                CreatedAt = dt,
                                UpdatedAt = dt
                            };
                            userPermission = await repoUserPermission.Create(paramCreateUserPermission, cancellationToken);
                        }
                    }

                    var paramCreateApiUrlPermission = new ApiUrlPermission
                    {
                        ApiUrlId = id,
                        UserPermissionId = userPermission!.Id,
                        CreatedAt = dt,
                        UpdatedAt = dt
                    };
                    await repoApiUrlPermission.Create(paramCreateApiUrlPermission, cancellationToken);
                }
            }
            scope.Complete();

            return new Response<object>()
            {
                Data = { },
                Log = logId!,
                Message = BasicConstants.SUCCCESS
            };

        }
        finally
        {
            await repoPgLock.ReleaseLock(lockKey, "", cancellationToken);
        }


    }

    public class ReqUpdateApiUrlRoute
    {
        public required string Url { get; set; }
        public required string Method { get; set; }
        public List<ReqUpdateApiUrlRouteApiUrlPermission>? ApiUrlPermissions { get; set; } = [];
    }

    public class ReqUpdateApiUrlRouteApiUrlPermission
    {
        public required string Id { get; set; }
        public required string UserPermissionId { get; set; }
        public required string UserPermissionName { get; set; }
    }

    public class ReqUpdateApiUrlRouteValidator : AbstractValidator<ReqUpdateApiUrlRoute>
    {
        public ReqUpdateApiUrlRouteValidator()
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
                 apiUrlPermission.RuleFor(i => i.UserPermissionId)
                     .NotEmpty()
                     .WithMessage("UserPermissionId is required");

                 apiUrlPermission.RuleFor(i => i.UserPermissionName)
                     .NotEmpty()
                     .WithMessage("UserPermissionName is required");

                 apiUrlPermission.RuleFor(i => i.Id)
                     .NotEmpty()
                     .WithMessage("Id is required");
             });
        }
    }
}