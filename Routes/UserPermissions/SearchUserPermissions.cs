

using System.Threading.Tasks;
using App.Constants;
using App.Extensions;
using App.Models;
using App.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.UserPermissions;

public static class SearchUserPermissionsRoute
{
    public static async Task<Response<ResUserPermissions>> Do(
        HttpContext context,
        IUserPermissionRepository repoUserPermission,
        CancellationToken cancellationToken,
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "page_size")] int? pageSize,
        [FromQuery(Name = "name")] string? name,
        [FromQuery(Name = "name_like")] string? nameLike
    )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var @params = new SearchUserPermissionsParams()
        {
            Page = page,
            PageSize = pageSize,
            Name = name,
            NameLike = nameLike
        };
        var dataCount = repoUserPermission.SearchCount(@params, cancellationToken);
        var data = await repoUserPermission.Search(@params, cancellationToken);
        return new Response<ResUserPermissions>()
        {
            Log = logId!,
            Message = BasicConstants.SUCCCESS,
            Data = new ResUserPermissions()
            {
                Meta = new ResponseMeta
                {
                    TotalRecord = await dataCount,
                    PageSize = pageSize ?? (await dataCount),
                    Page = page ?? 1,
                },
                Items = data?.Select(x => new ResUserPermissionsData()
                {
                    Id = x.Id,
                    Name = x.Name,
                    UpdatedAt = x.UpdatedAt.ToDefaultString(),
                    CreatedAt = x.CreatedAt.ToDefaultString(),
                }) ?? []
            }
        };
    }

    public class ResUserPermissionsData
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
    }

    public class ResUserPermissions
    {
        public required IEnumerable<ResUserPermissionsData> Items { get; set; }
        public required ResponseMeta Meta { get; set; }
    }
}