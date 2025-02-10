

using System.Threading.Tasks;
using App.Constants;
using App.Models;
using App.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace App.Routes.Users;


public static class DeleteUserRoute
{
    public static async Task<object> Do(
        HttpContext context,
        [FromRoute] string id,
        [FromServices] IPgLockRepository repoPgLock,
        [FromServices] IUserRepository repoUser,
        CancellationToken cancellationToken
    )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var lockKey = $"{nameof(DeleteUserRoute)}-{id}";
        try
        {
            await repoPgLock.GetLock(lockKey, "", TimeSpan.FromSeconds(15), cancellationToken);

            var paramSearch = new SearchUsersParams { Id = id };
            var data = await repoUser.Search(paramSearch, cancellationToken);
            if (data == null || data.IsNullOrEmpty())
                throw new NotFoundException(code: ErrorConstants.API_USER_DELETE_USER_ERROR, "User not found");

            await repoUser.Delete(id, cancellationToken);
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