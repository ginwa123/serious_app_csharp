

using App.Constants;
using App.Models;
using App.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.ApiUrls;


public static class GetApiUrlByIdRoute
{
    public static async Task<object> Do(
        [FromServices] IApiUrlRepository repoApiUrl,
        [FromServices] IApiUrlPermissionRepository repoApiUrlPermission,
        [FromRoute(Name = "id")] string id,
        HttpContext context,
        CancellationToken cancellationToken
    )
    {
        var data = await SearchApiUrlsRoute.Do(
            context: context,
            repoApiUrl: repoApiUrl,
            id: id,
            urls: null,
            pageSize: null,
            page: null,
            method: null,
            methods: null,
            methodLike: null,
            urlLike: null,
            ids: null,
            repoApiUrlPermission: repoApiUrlPermission,
            url: null,
            cancellationToken: cancellationToken
        );

        var dataApiUrl = data?.Data?.Items.FirstOrDefault();
        if (dataApiUrl == null)
            throw new NotFoundException(code: ErrorConstants.API_URL_GET_API_URL_BY_ID_ERROR, "ApiUrl not found");

        return new Response<object>
        {
            Log = data?.Log!,
            Message = BasicConstants.SUCCCESS,
            Data = dataApiUrl
        };
    }

}