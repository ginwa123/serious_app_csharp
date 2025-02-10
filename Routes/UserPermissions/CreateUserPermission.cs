
using System.Text.Json;
using App.Constants;
using App.Models;
using App.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.UserPermissions;

public static class CreateUserPermissionRoute
{
    public static async Task<object> Do(
         HttpContext context,
         [FromServices] IUserPermissionRepository repoUserPermission,
         [FromServices] IPgLockRepository repoPgLock,
         [FromBody] ReqUserPermissions request,
         CancellationToken cancellationToken
     )
    {
        var lockKey = $"{nameof(CreateUserPermissionRoute)}-{JsonSerializer.Serialize(request)}";
        var logId = context.Items[LogConstants.LOG]?.ToString();

        try
        {
            var successGetLock = await repoPgLock.GetLock(lockKey, "", TimeSpan.FromSeconds(15), cancellationToken);
            if (!successGetLock)
                throw new UnhandledException(code: ErrorConstants.FAILED_GET_LOCK, $"Failed to get lock {nameof(CreateUserPermissionRoute)}");

            var validator = new ReqUserPermissionValidator().Validate(request);
            if (!validator.IsValid)
                throw new BadRequestException(validator.Errors);


            var @params = new SearchUserPermissionsParams()
            {
                Name = request.Name
            };
            var count = await repoUserPermission.SearchCount(@params, cancellationToken);
            if (count > 0)
                throw new ConflictException(code: ErrorConstants.API_USER_PERMISSION_CREATE_ERROR_DUPLICATE, $"Duplicate {nameof(CreateUserPermissionRoute)}");

            var dt = DateTime.UtcNow;
            var objBody = new UserPermission()
            {
                Name = request.Name,
                CreatedAt = dt,
                UpdatedAt = dt
            };
            await repoUserPermission.Create(objBody, cancellationToken);

            return new Response<object>()
            {
                Log = logId!,
                Message = BasicConstants.SUCCCESS
            };
        }
        finally
        {
            await repoPgLock.ReleaseLock(lockKey, "", cancellationToken);
        }

    }

    public class ReqUserPermissions
    {
        public required string Name { get; set; }
    }

    public class ReqUserPermissionValidator : AbstractValidator<ReqUserPermissions>
    {
        public ReqUserPermissionValidator()
        {
            RuleFor(i => i.Name).NotEmpty().MinimumLength(1);
        }
    }
}