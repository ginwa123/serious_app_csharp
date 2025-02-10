

using System.Text.Json;
using System.Transactions;
using App.Constants;
using App.Models;
using App.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.Users;


public static class UpdateUserRoute
{
    public static async Task<object> Do(
       HttpContext context,
        [FromServices] IPgLockRepository repoPgLock,
        [FromBody] ReqUpdateUser request,
        [FromServices] IUserRepository repoUser,
        [FromRoute] string id,
        CancellationToken cancellationToken
    )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var lockKey = $"{nameof(UpdateUserRoute)}{JsonSerializer.Serialize(request)}";
        try
        {
            await repoPgLock.GetLock(lockKey, "", TimeSpan.FromSeconds(15), cancellationToken);
            // validate signedin
            var signedUser = context.Items[BasicConstants.SIGNED_USER] as SignedUser;
            if (signedUser!.Id != id)
            {
                if (signedUser.RoleAssignee.Where(x => x?.Name == "admin").Any())
                {
                    // admin can do anything then do nothing
                }
                else
                {
                    throw new UnauthorizedException(code: ErrorConstants.API_USER_UPDATE_ERROR_INVALID_USER_SIGNED, "Cannot update other user");
                }
            }

            var validator = new ReqUpdateUserValidator().Validate(request);
            if (!validator.IsValid)
                throw new BadRequestException(validator.Errors);

            // validate by id
            var existingUser = await repoUser.Search(new SearchUsersParams { Id = id }, cancellationToken);
            if (existingUser.FirstOrDefault() == null)
                throw new NotFoundException(code: ErrorConstants.API_USER_UPDATE_ERROR_USER_NOT_FOUND, "User not found");

            // validate new username
            var checkDataUser = await repoUser.Search(new SearchUsersParams { Username = request.Username }, cancellationToken);
            if (checkDataUser != null)
            {
                checkDataUser = checkDataUser.Where(x => x.Id != id);
                if (checkDataUser.Any())
                    throw new ConflictException(code: ErrorConstants.API_USER_UPDATE_ERROR_DUPLICATE, "Duplicate user cannot update");
            }

            var isValidPassword = BCrypt.Net.BCrypt.Verify(request.OldPassword, existingUser.FirstOrDefault()?.Password);
            if (!isValidPassword)
                throw new UnhandledException(code: ErrorConstants.API_USER_UPDATE_ERROR_INVALID_PASSWORD, "Invalid password");

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var user = new User
            {
                Id = id,
                Name = request?.Name ?? existingUser.FirstOrDefault()?.Name ?? string.Empty,
                Username = request?.Username ?? existingUser.FirstOrDefault()?.Username ?? string.Empty,
                Password = BCrypt.Net.BCrypt.HashPassword(request!.NewPassword1),
                UpdatedAt = DateTime.UtcNow,
            };

            await repoUser.Update(user, cancellationToken);
            scope.Complete();

            return new Response<object>()
            {
                Log = logId!,
                Message = BasicConstants.SUCCCESS
            };
        }
        finally
        {
            await repoPgLock.ReleaseLock(lockKey, "", cancellationToken);
        }
    }

    public class ReqUpdateUser
    {
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? NewPassword1 { get; set; }
        public string? NewPassword2 { get; set; }
        public string? OldPassword { get; set; }

    }

    public class ReqUpdateUserValidator : AbstractValidator<ReqUpdateUser>
    {
        public ReqUpdateUserValidator()
        {
            RuleFor(i => i.Name).NotEmpty().MinimumLength(1);
            RuleFor(i => i.Username).NotEmpty().MinimumLength(4);
            RuleFor(i => i.NewPassword1).NotEmpty().MinimumLength(4);
            RuleFor(i => i.NewPassword2).NotEmpty().MinimumLength(4);
            RuleFor(i => i.OldPassword).NotEmpty().MinimumLength(4);

            RuleFor(i => i.NewPassword1).Equal(i => i.NewPassword2);
        }
    }
}