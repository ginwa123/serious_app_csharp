using App.Constants;
using App.Models;
using App.Pkg;
using App.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.Users.Auth;


public static class AuthSignInAdminRoute
{
    public static async Task<User> BaseSignIn(
        HttpContext context,
         IUserRepository repoUser,
         IJWTPkg jwt,
         ReqAuthSignIn request,
         IUserRoleAssigneeRepository repoUserRoleAssignee,
         CancellationToken cancellationToken
    )
    {
        var validator = new ReqAuthSignInValidator();
        var validatorErros = validator.Validate(request);
        if (!validatorErros.IsValid)
            throw new BadRequestException(validatorErros.Errors);

        var user = (await repoUser.Search(new SearchUsersParams { Username = request.Username }, cancellationToken)).FirstOrDefault()
            ?? throw new NotFoundException(code: ErrorConstants.SIGN_IN_FAILED_USER_NOT_FOUND, "User not found");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            throw new UnauthorizedException(code: ErrorConstants.SIGN_IN_FAILED_INVALID_PASSWORD, "Invalid password");

        var par = new SearchUserRoleAssigneesParams { UserId = user.Id };
        var dataAsyncUserRoleAssigneed = await repoUserRoleAssignee.Search(par, cancellationToken);

        user.UserRoleAssignees = dataAsyncUserRoleAssigneed?.Select(x =>
        {
            return new UserRoleAssignee
            {
                Id = x?.Id ?? string.Empty,
                UserId = x?.UserId ?? string.Empty,
                UserRoleId = x?.UserRoleId ?? string.Empty,
                UserRoleName = x?.UserRoleName ?? string.Empty
            };
        }).ToArray() ?? [];

        return user;
    }

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

        var user = await BaseSignIn(context, repoUser, jwt, request, repoUserRoleAssignee, cancellationToken);
        if (user.UserRoleAssignees?.FirstOrDefault(x => x.UserRoleName == "admin") == null)
            throw new UnauthorizedException(code: ErrorConstants.SIGN_IN_FAILED_USER_NOT_ADMIN, "User not admin");

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
                RefreshToken = refreshToken
            },
            Log = logId ?? ""
        };
    }
}
