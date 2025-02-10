

using App.Constants;
using App.Models;
using App.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.UserPermissions;

public static class DeleteUserPermissionRoute
{
    public static async Task<object> Do(
       [FromRoute] string id,
       HttpContext context,
       [FromServices] IPgLockRepository repoPgLock,
       [FromServices] IUserPermissionRepository repoUserPermission,
       CancellationToken cancellationToken
   )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var lockKey = $"{nameof(DeleteUserPermissionRoute)}-{id}";
        try
        {
            var successGetLock = await repoPgLock.GetLock(lockKey, "", TimeSpan.FromSeconds(15), cancellationToken);
            if (!successGetLock)
                throw new UnhandledException(code: ErrorConstants.FAILED_GET_LOCK, $"Failed to get lock {nameof(DeleteUserPermissionRoute)}");

            var @params = new SearchUserPermissionsParams()
            {
                Id = id
            };

            var count = await repoUserPermission.SearchCount(@params, cancellationToken);
            if (count == 0)
                throw new NotFoundException(code: ErrorConstants.API_USER_PERMISSION_DELETE_ERROR_NOT_FOUND, $"Not found {nameof(DeleteUserPermissionRoute)}");

            await repoUserPermission.Delete(id, cancellationToken);
            return new Response<object>()
            {
                Log = logId!,
                Message = BasicConstants.SUCCCESS,
                Data = null
            };
        }
        finally
        {
            await repoPgLock.ReleaseLock(lockKey, "", cancellationToken);
        }
    }
}