using App.Constants;
using App.Extensions;
using App.Models;
using App.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.Users;

public static class GetUserByIdRoute
{
    public class ResGetUserById
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Username { get; set; }
        public required string CreatedAt { get; set; }
        public required string UpdatedAt { get; set; }
    }
    public static async Task<Response<ResGetUserById>> Do(
         HttpContext context,
         [FromServices] IUserRepository repoUser,
         [FromRoute(Name = "id")] string id,
         CancellationToken cancellationToken
    )
    {

        var logId = context.Items[LogConstants.LOG]?.ToString();

        var par = new SearchUsersParams { Id = id };
        var user = (await repoUser.Search(par, cancellationToken)).FirstOrDefault()
            ?? throw new NotFoundException(code: ErrorConstants.GET_USER_BY_ID_ERROR, "User not found");

        var mapping = new ResGetUserById
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            CreatedAt = user.CreatedAt.ToDefaultString(),
            UpdatedAt = user.UpdatedAt.ToDefaultString(),
        };
        var res = new Response<ResGetUserById>
        {
            Log = logId!,
            Message = "success",
            Data = mapping
        };

        return res;
    }
}