
using System.Text.Json;
using System.Transactions;
using App.Constants;
using App.Models;
using App.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace App.Routes.Users;


public static class CreateUserRoute
{
    public static async Task<object> Do(
        HttpContext context,
        [FromServices] IPgLockRepository repoPgLock,
        [FromBody] ReqCreateUser request,
        [FromServices] IUserRepository repoUser,
        [FromServices] IUserRoleAssigneeRepository repoUserRoleAssignee,
        [FromServices] IUserRoleRepository repoUserRole,
        CancellationToken cancellationToken
    )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var lockKey = $"{nameof(CreateUserRoute)}{JsonSerializer.Serialize(request)}";
        try
        {
            await repoPgLock.GetLock(lockKey, "", TimeSpan.FromSeconds(15), cancellationToken);
            var validator = new ReqCreateUserValidator().Validate(request);
            if (!validator.IsValid)
                throw new BadRequestException(validator.Errors);

            var userCount = await repoUser.SearchCount(new SearchUsersParams { Username = request.Username }, cancellationToken);
            if (userCount > 0)
                throw new ConflictException(code: ErrorConstants.API_USER_CREATE_ERROR_DUPLICATE, "Duplicate user cannot create");

            var dataRole = await repoUserRole.Search(new SearchUserRolesParams { Name = "member" }, cancellationToken);
            if (dataRole == null || dataRole.IsNullOrEmpty())
                throw new NotFoundException(code: ErrorConstants.API_USER_CREATE_ERROR_NO_MEMBER_ROLE, "No member role found");

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var user = new User
            {
                Name = request.Name,
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var newCreatedUser = await repoUser.Create(user, cancellationToken);

            await repoUserRoleAssignee.Create(new UserRoleAssignee
            {
                UserId = newCreatedUser?.Id ?? string.Empty,
                UserRoleId = dataRole.FirstOrDefault()?.Id ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, cancellationToken);

            scope.Complete();
            return new Response<string>()
            {
                Log = logId!,
                Message = BasicConstants.SUCCCESS,
                Data = newCreatedUser!.Id
            };
        }
        finally
        {
            await repoPgLock.ReleaseLock(lockKey, "", cancellationToken);
        }
    }

    public class ReqCreateUser
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }


    public class ReqCreateUserValidator : AbstractValidator<ReqCreateUser>
    {
        public ReqCreateUserValidator()
        {
            RuleFor(i => i.Name).NotEmpty().MinimumLength(1);
            RuleFor(i => i.Username).NotEmpty().MinimumLength(4);
            RuleFor(i => i.Password).NotEmpty().MinimumLength(4);
        }
    }
}