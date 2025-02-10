using App.Constants;
using App.Models;
using App.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.Users;

public static class SearchUserRoute
{
    public static async Task<object> Do(
        [FromServices] IUserRepository userRepository,
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "page_size")] int? pageSize,
        [FromQuery(Name = "username")] string? username,
        HttpContext context,
        CancellationToken cancellationToken
    )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var par = new SearchUsersParams
        {
            Page = page,
            PageSize = pageSize,
        };
        var data = userRepository.Search(par, cancellationToken);
        var dataCount = userRepository.SearchCount(par, cancellationToken);

        var res = new Response<object>
        {
            Log = logId!,
            Message = "success",
            Data = new
            {
                Items = await data,
                Meta = new ResponseMeta
                {
                    TotalRecord = await dataCount,
                    PageSize = pageSize ?? await dataCount,
                    Page = page ?? 1,
                }
            }
        };


        return res;
    }
}