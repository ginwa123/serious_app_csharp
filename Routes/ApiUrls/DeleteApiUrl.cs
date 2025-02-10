using App.Constants;
using App.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.Repositories;


public static class DeleteApiUrlRoute
{
    public static async Task<object> Do(
        [FromRoute] string id,
        [FromServices] IApiUrlRepository repoApiUrl,
        HttpContext context,
        [FromServices] IPgLockRepository repoPgLock,
        CancellationToken cancellationToken
    )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var lockKey = $"{nameof(DeleteApiUrlRoute)}-{id}";
        try
        {
            var successGetLock = await repoPgLock.GetLock(lockKey, "", TimeSpan.FromSeconds(15), cancellationToken);
            if (!successGetLock)
                throw new UnhandledException(code: ErrorConstants.FAILED_GET_LOCK, $"Failed to get lock {nameof(DeleteApiUrlRoute)}");

            var paramSearch = new SearchApiUrlsParams { Id = id };
            var data = await repoApiUrl.Search(paramSearch, cancellationToken);
            if (data == null)
                throw new NotFoundException(code: ErrorConstants.API_URL_DELETE_API_URL_ERROR, "ApiUrl not found");

            var _ = await repoApiUrl.Delete(id, cancellationToken);
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