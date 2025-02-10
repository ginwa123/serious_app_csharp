using System.Transactions;
using App.Constants;
using App.Models;
using App.Pkg;
using App.Repositories;
using App.Routes.ApiUrls;
using App.Routes.UserPermissions;
using App.Routes.Users;
using App.Routes.Users.Auth;
using App.Routes.Workspaces;
using JWT.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace App.Routes
{
    public static class Routes
    {

        public static RouteGroupBuilder MapUsersApi(this RouteGroupBuilder group)
        {
            group.MapGet("/", SearchUserRoute.Do)
                .JWTValidation();
            group.MapGet("/{id}", GetUserByIdRoute.Do)
                .JWTValidation();
            group.MapPost("/", CreateUserRoute.Do);
            group.MapPut("/{id}", UpdateUserRoute.Do)
                .JWTValidation();
            group.MapDelete("/{id}", DeleteUserRoute.Do)
                .JWTValidation();

            return group;
        }

        public static RouteGroupBuilder MapUsersAuthApi(this RouteGroupBuilder group)
        {
            group.MapPost("/sign-in", AuthSignInRoute.Do);
            group.MapPost("/admin/sign-in", AuthSignInAdminRoute.Do);
            group.MapPost("/admin/check", AuthCheckRoute.Do)
                .JWTValidation(isAdmin: true);
            return group;
        }

        public static RouteGroupBuilder MapWorkspacesApi(this RouteGroupBuilder group)
        {
            group.MapPost("/", CreateWorkspaceRoute.Do)
                .JWTValidation();
            group.MapGet("/", SearchWorkspaceRoute.Do)
                .JWTValidation();
            group.MapGet("/{id}", GetWorkspaceByIdRoute.Do)
                .JWTValidation();
            // group.MapDelete("/{id}", DeleteWorkspaceRoute.Do)
            //     .JWTValidation(isAdmin: true);
            // group.MapPut("/{id}", UpdateWorkspaceRoute.Do)
            //     .JWTValidation(isAdmin: true);
            return group;
        }

        public static RouteGroupBuilder MapApiUrls(this RouteGroupBuilder group)
        {
            group.MapPost("/", CreateApiUrlRoute.Do)
                .JWTValidation(isAdmin: true);
            group.MapGet("/", SearchApiUrlsRoute.Do)
                .JWTValidation(isAdmin: true);
            group.MapGet("/{id}", GetApiUrlByIdRoute.Do)
                .JWTValidation(isAdmin: true);
            group.MapDelete("/{id}", DeleteApiUrlRoute.Do)
                .JWTValidation(isAdmin: true);
            group.MapPut("/{id}", UpdateApiUrlRoute.Do)
               .JWTValidation(isAdmin: true);
            return group;
        }

        public static RouteGroupBuilder MapUserPermissionsApi(this RouteGroupBuilder group)
        {
            group.MapGet("/", SearchUserPermissionsRoute.Do)
                .JWTValidation(isAdmin: true);
            group.MapPost("/", CreateUserPermissionRoute.Do)
                .JWTValidation(isAdmin: true);
            group.MapDelete("/{id}", DeleteUserPermissionRoute.Do)
                .JWTValidation(isAdmin: true);
            return group;
        }

        public static RouteGroupBuilder MapLocks(this RouteGroupBuilder group)
        {
            group.MapPost("/unlock-all", async (
                [FromServices] IPgLockRepository repoPgLock,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var log = context.Items[LogConstants.LOG]?.ToString();
                await repoPgLock.ReleaseAllLockAsync(cancellationToken);

                return new Response<object>()
                {
                    Message = BasicConstants.SUCCCESS,
                    Log = log!
                };
            })
            .JWTValidation(isAdmin: true);

            group.MapGet("/", async (
             [FromServices]
             IPgLockRepository repoPgLock,
            HttpContext context,
            CancellationToken cancellationToken
                ) =>
            {
                var log = context.Items[LogConstants.LOG]?.ToString();
                var data = (await repoPgLock.Search(cancellationToken)).ToList();

                return new Response<object>()
                {
                    Message = BasicConstants.SUCCCESS,
                    Log = log!,
                    Data = data
                };
            }).JWTValidation(isAdmin: true);

            return group;
        }



        public static RouteHandlerBuilder JWTValidation(this RouteHandlerBuilder handler, bool isAdmin = false)
        {
            handler.AddEndpointFilter(async (context, next) =>
            {
                var cancellationToken = context.HttpContext.RequestAborted;
                var repoUser = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var repoUserPermissionAssigne = context.HttpContext.RequestServices.GetRequiredService<IUserRoleAssigneeRepository>();
                var repoApiUrlPermission = context.HttpContext.RequestServices.GetRequiredService<IApiUrlPermissionRepository>();
                var repoUserPermission = context.HttpContext.RequestServices.GetRequiredService<IUserPermissionRepository>();
                var repoUserRolePermission = context.HttpContext.RequestServices.GetRequiredService<IUserRolePermissionRepository>();
                var pkgJwt = context.HttpContext.RequestServices.GetRequiredService<IJWTPkg>();
                var repoApiUrl = context.HttpContext.RequestServices.GetRequiredService<IApiUrlRepository>();

                var access_token = context.HttpContext.Request.Headers.Authorization.ToString()?.Replace("Bearer ", "");
                if (string.IsNullOrEmpty(access_token))
                    access_token = context.HttpContext.Request.Cookies["access_token"]?.ToString()
                    ?? context.HttpContext.Request.Headers["X-Access-Token"].ToString() ?? string.Empty;

                var refresh_token = context.HttpContext.Request.Cookies["refresh_token"]?.ToString()
                ?? context.HttpContext.Request.Headers["X-Refresh-Token"].ToString() ?? string.Empty;


                Console.WriteLine("access_token: " + access_token);
                Console.WriteLine("refresh_token: " + refresh_token);
                if (string.IsNullOrEmpty(access_token) && string.IsNullOrEmpty(refresh_token))
                    throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "Token is empty");

                JWTClaim? jwtClaim = null;
                try
                {
                    if (!string.IsNullOrEmpty(access_token))
                    {
                        jwtClaim = pkgJwt.DecodeJwtToken(access_token);
                        if (jwtClaim?.Type != "access_token")
                            throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "Invalid Type");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(refresh_token))
                            throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "Token is empty");

                        throw new TokenExpiredException("empty token");
                    }

                }
                catch (TokenNotYetValidException)
                {
                    throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "Token not valid");
                }
                catch (TokenExpiredException)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(refresh_token))
                            throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "Refresh token is empty");

                        jwtClaim = pkgJwt.DecodeJwtToken(refresh_token ?? string.Empty);
                        if (jwtClaim?.Type != "refresh_token")
                            throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "Invalid Type Not Refresh Token");

                        // generate new access token and refresh token
                        var (token, exp) = pkgJwt.GenerateJwtToken(jwtClaim.Username, "access_token");
                        var (refreshToken, refreshExp) = pkgJwt.GenerateJwtToken(jwtClaim.Username, "refresh_token");

                        var cookieAccessOptions = new CookieOptions
                        {
                            HttpOnly = false, // Cookie can't be accessed via JavaScript
                            Secure = false,   // Cookie will be sent only over HTTPS
                            SameSite = SameSiteMode.Lax, // Restrict cross-site cookie sending
                            Expires = exp // Set expiration time
                        };
                        context.HttpContext.Response.Cookies.Append("access_token", token, cookieAccessOptions);

                        var cookieRefreshOptions = new CookieOptions
                        {
                            HttpOnly = false, // Cookie can't be accessed via JavaScript
                            Secure = false,   // Cookie will be sent only over HTTPS
                            SameSite = SameSiteMode.Lax, // Restrict cross-site cookie sending
                            Expires = refreshExp // Set expiration time
                        };
                        context.HttpContext.Response.Cookies.Append("refresh_token", refreshToken, cookieRefreshOptions);
                        context.HttpContext.Response.Headers["X-Access-Token"] = token;
                        context.HttpContext.Response.Headers["X-Refresh-Token"] = refreshToken;

                    }
                    catch (System.Exception)
                    {
                        throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "Token expired");
                    }

                    throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "Token expired");
                }
                catch (SignatureVerificationException)
                {
                    throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "Token signature not valid");
                }
                catch (System.Exception)
                {
                    throw;
                }
                finally
                {
                    if (jwtClaim == null)
                        throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "JWT Claim is null");

                    var datauser = (await repoUser.Search(new SearchUsersParams { Username = jwtClaim.Username }, cancellationToken)).FirstOrDefault();
                    if (datauser == null)
                        throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "User not found");

                    var dataUserPermissionAssignet = (await repoUserPermissionAssigne.Search(new SearchUserRoleAssigneesParams
                    {
                        UserId = datauser.Id
                    }, cancellationToken));

                    if (isAdmin)
                    {
                        if (dataUserPermissionAssignet.FirstOrDefault(x => x.UserRoleName == "admin") == null)
                        {
                            throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, "User not admin");
                        }
                    }

                    // check the url is registered
                    var url = context.HttpContext.Request.Path.Value;
                    var routeValues = context.HttpContext.Request.RouteValues;
                    foreach (var value in routeValues.Values)
                    {
                        url = url!.Replace("/" + value, "");
                    }
                    var method = context.HttpContext.Request.Method.ToLower();

                    var apiUrl = (await repoApiUrl.Search(new SearchApiUrlsParams { Url = url, Method = method }, cancellationToken)).ToList();
                    if (apiUrl.IsNullOrEmpty())
                    {
                        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                        var userPermissionDefault = await repoUserPermission.Search(new SearchUserPermissionsParams { Names = UserPermission.MandatorUserPermissions }, cancellationToken);
                        var dictUserPermission = new Dictionary<string, UserPermission>
                        {
                            { "get", userPermissionDefault.First(x => x.Name == "can_readall") },
                            { "post", userPermissionDefault.First(x => x.Name == "can_createall") },
                            { "put", userPermissionDefault.First(x => x.Name == "can_updateall") },
                            { "delete", userPermissionDefault.First(x => x.Name == "can_deleteall") }
                        };

                        // if null create the api url
                        var newCreatedApiUrl = await repoApiUrl.Create(new ApiUrl
                        {
                            Url = url!,
                            Method = method,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }, cancellationToken);
                        apiUrl.Add(newCreatedApiUrl!);

                        // then create the api url Permission too
                        await repoApiUrlPermission.Create(new ApiUrlPermission()
                        {
                            ApiUrlId = newCreatedApiUrl!.Id,
                            UserPermissionId = dictUserPermission[method].Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }, cancellationToken);

                        scope.Complete();
                    }

                    // check the user has permission to access the api url
                    // var userData = await repoUser.Search(new SearchUsersParams { Id = user.Id }, cancellationToken);
                    var apiUrlPermission = await repoApiUrlPermission.SearchApiUrlPermissions(
                        new SearchApiUrlPermissionsParams { ApiUrlId = apiUrl!.First().Id },
                    cancellationToken);

                    if (apiUrlPermission.IsNullOrEmpty())
                    {
                        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                        var userPermissionDefault = await repoUserPermission.Search(new SearchUserPermissionsParams { Names = UserPermission.MandatorUserPermissions }, cancellationToken);
                        var dictUserPermission = new Dictionary<string, UserPermission>
                        {
                            { "get", userPermissionDefault.First(x => x.Name == "can_readall") },
                            { "post", userPermissionDefault.First(x => x.Name == "can_createall") },
                            { "put", userPermissionDefault.First(x => x.Name == "can_updateall") },
                            { "delete", userPermissionDefault.First(x => x.Name == "can_deleteall") }
                        };

                        await repoApiUrlPermission.Create(new ApiUrlPermission()
                        {
                            ApiUrlId = apiUrl.First()!.Id,
                            UserPermissionId = dictUserPermission[method].Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }, cancellationToken);

                        scope.Complete();

                        apiUrlPermission = await repoApiUrlPermission.SearchApiUrlPermissions(
                        new SearchApiUrlPermissionsParams { ApiUrlId = apiUrl!.First().Id }, cancellationToken);
                    }

                    var userRoleIds = dataUserPermissionAssignet.Select(x => x.UserRoleId).ToArray();
                    var dataUserRolePermission = await repoUserRolePermission.Search(new SearchUserRolerPermissionParams()
                    {
                        UserRoleIds = userRoleIds
                    }, cancellationToken);
                    // check the permission of the user and the url

                    var isFoundThePermission = false;
                    foreach (var apiUrlPerm in apiUrlPermission)
                    {
                        var userPermissionId = apiUrlPerm.UserPermissionId;
                        var isFound = dataUserRolePermission.FirstOrDefault(x => x.UserPermissionId == userPermissionId);
                        if (isFound != null)
                        {
                            isFoundThePermission = true;
                            break;
                        }
                    }

                    if (!isFoundThePermission)
                    {
                        throw new UnauthorizedException(code: ErrorConstants.NO_TOKEN_AUTHORIZATION, $"User not authorized access url {url}, method {method}");
                    }

                    // context.Items["ss"] = ""
                    context.HttpContext.Items[BasicConstants.SIGNED_USER] = new SignedUser
                    {
                        Id = datauser.Id,
                        Name = datauser.Name,
                        Username = datauser.Username,
                        RoleAssignee = dataUserPermissionAssignet?.Select(x =>
                        {
                            return new SignedUserRoleAssignee()
                            {
                                Name = x?.UserRoleName ?? string.Empty
                            };
                        }).ToArray() ?? []
                    };

                }

                return await next(context);
            });
            return handler;
        }
    }
}