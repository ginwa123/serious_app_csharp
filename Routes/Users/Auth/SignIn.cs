using App.Constants;
using App.Models;
using App.Pkg;
using App.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.Users.Auth;


public static class AuthSignInRoute
{
    public static async Task<object> Do(
        HttpContext context,
        [FromServices] IUserRepository repoUser,
        [FromServices] IJWTPkg jwt,
        [FromBody] ReqAuthSignIn request,
        [FromServices] IUserRoleAssigneeRepository repoUserRoleAssignee,
        CancellationToken cancellationToken
    )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var user = await AuthSignInAdminRoute.BaseSignIn(context, repoUser, jwt, request, repoUserRoleAssignee, cancellationToken);


        var (token, exp) = jwt.GenerateJwtToken(user.Username, "access_token");
        var (refreshToken, refreshExp) = jwt.GenerateJwtToken(user.Username, "refresh_token");

        var cookieAccessOptions = new CookieOptions
        {
            HttpOnly = false, // Cookie can't be accessed via JavaScript
            Secure = false,   // Cookie will be sent only over HTTPS
            SameSite = SameSiteMode.Lax, // Restrict cross-site cookie sending
            Expires = exp // Set expiration time
        };

        context.Response.Cookies.Append("access_token", token, cookieAccessOptions);

        var cookieRefreshOptions = new CookieOptions
        {
            HttpOnly = false, // Cookie can't be accessed via JavaScript
            Secure = false,   // Cookie will be sent only over HTTPS
            SameSite = SameSiteMode.Lax, // Restrict cross-site cookie sending
            Expires = refreshExp // Set expiration time
        };
        context.Response.Cookies.Append("refresh_token", refreshToken, cookieRefreshOptions);

        context.Response.Headers["X-Access-Token"] = token;
        context.Response.Headers["X-Refresh-Token"] = refreshToken;

        return new Response<object>()
        {
            Message = "ok",
            Data = new
            {
                Username = user.Username,
                UserId = user.Id,
                AccessToken = token,
                RefreshToken = refreshToken,
                UserRoleAssignee = user.UserRoleAssignees
            },
            Log = logId ?? ""
        };
    }
}

public class ReqAuthSignIn
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class ReqAuthSignInValidator : AbstractValidator<ReqAuthSignIn>
{
    public ReqAuthSignInValidator()
    {
        RuleFor(i => i.Username).NotEmpty().MinimumLength(1);

        RuleFor(i => i.Password).NotEmpty();
    }
}