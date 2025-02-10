
using App.Constants;
using App.Extensions;
using App.Models;
using App.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.ApiUrls;


public static class SearchApiUrlsRoute
{

    public class ResSearchApiUrlRoute
    {
        public required IEnumerable<ResSearchApiUrlRouteDetail> Items { get; set; }
        public required ResponseMeta Meta { get; set; }
    }

    public class ResSearchApiUrlRouteDetail
    {
        public required string Id { get; set; }
        public required string Url { get; set; }
        public required string Method { get; set; }
        public required string CreatedAt { get; set; }
        public required string UpdatedAt { get; set; }
        public required ResSearchApiUrlRouteDetailPermission[] ApiUrlPermissions { get; set; }

    }

    public class ResSearchApiUrlRouteDetailPermission
    {
        public required string Id { get; set; }
        public required string ApiUrlId { get; set; }
        public required string UserPermissionId { get; set; }
        public required string UserPermissionName { get; set; }
    }
    public static async Task<Response<ResSearchApiUrlRoute>> Do(
        HttpContext context,
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "page_size")] int? pageSize,
        [FromQuery(Name = "url")] string? url,
        [FromQuery(Name = "urls")] string? urls,
        [FromQuery(Name = "url_like")] string? urlLike,
        [FromQuery(Name = "method")] string? method,
        [FromQuery(Name = "methods")] string? methods,
        [FromQuery(Name = "method_like")] string? methodLike,
        [FromQuery(Name = "id")] string? id,
        [FromQuery(Name = "ids")] string? ids,
        [FromServices] IApiUrlRepository repoApiUrl,
        [FromServices] IApiUrlPermissionRepository repoApiUrlPermission,
        CancellationToken cancellationToken
    )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var par = new SearchApiUrlsParams
        {
            Id = id,
            Ids = ids?.Split(",") ?? null,
            Url = url,
            Urls = urls?.Split(",") ?? null,
            Method = method,
            Page = page,
            PageSize = pageSize
        };
        var dataApiUrls = await repoApiUrl.Search(par, cancellationToken);
        var dataApiUrlsCount = await repoApiUrl.SearchCount(par, cancellationToken);

        var parApiUrlPermission = new SearchApiUrlPermissionsParams
        {
            ApiUrlIds = [.. dataApiUrls.Select(x => x.Id)]
        };
        var dataAsyncApiUrlPermission = await repoApiUrlPermission.SearchApiUrlPermissions(parApiUrlPermission, cancellationToken);

        var mapItem = dataApiUrls?.ToList()?.Select(x =>
        {
            var dataApiUserPermission = dataAsyncApiUrlPermission?.Where(y => y.ApiUrlId == x.Id).ToList();

            var apiUrlPermissions = dataApiUserPermission?.Select(y => new ResSearchApiUrlRouteDetailPermission
            {
                Id = y?.Id ?? string.Empty,
                ApiUrlId = y?.ApiUrlId ?? string.Empty,
                UserPermissionId = y?.UserPermissionId ?? string.Empty,
                UserPermissionName = y?.UserPermissionName ?? string.Empty
            }).ToArray();
            return new ResSearchApiUrlRouteDetail
            {
                Id = x?.Id ?? string.Empty,
                Url = x?.Url ?? string.Empty,
                Method = x?.Method ?? string.Empty,
                CreatedAt = x?.CreatedAt.ToDefaultString() ?? string.Empty,
                UpdatedAt = x?.UpdatedAt.ToDefaultString() ?? string.Empty,
                ApiUrlPermissions = apiUrlPermissions ?? []
            };
        }
        ) ?? [];


        var Data = new ResSearchApiUrlRoute
        {
            Items = mapItem,
            Meta = new ResponseMeta
            {
                TotalRecord = dataApiUrlsCount,
                PageSize = pageSize ?? dataApiUrlsCount,
                Page = page ?? 1,
            }
        };

        var res = new Response<ResSearchApiUrlRoute>
        {
            Message = BasicConstants.SUCCCESS,
            Data = Data,
            Log = logId ?? string.Empty
        };
        return res;
    }
}