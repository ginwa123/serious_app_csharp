


using System.Text.Json;
using App.Constants;
using App.Models;
using App.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.Workspaces;


public static class CreateWorkspaceRoute
{
    internal static async Task Do(
        HttpContext context,
        [FromBody] ReqCreateWorkspace request,
        [FromServices] IPgLockRepository repoPgLock,
        [FromServices] IWorkspaceRepository repoWorkspace,
        [FromServices] IUserRepository repoUser,
        [FromServices] IWorkspaceParticipantRoleRepository repoWorkspaceParticipantRole,
        CancellationToken cancellationToken
        )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var lockKey = $"{nameof(CreateWorkspaceRoute)}-{JsonSerializer.Serialize(request)}";
        try
        {
            await repoPgLock.GetLock(lockKey, "", TimeSpan.FromSeconds(15), cancellationToken);
            var validator = new ReqCreateWorkspaceValidator().Validate(request);
            if (!validator.IsValid)
                throw new BadRequestException(validator.Errors);

            foreach (var participant in request.Participants)
            {
                var dataUser = repoUser.Search(new SearchUsersParams { Id = participant.UserId }, cancellationToken);
                var dataWorkspaceParticipantRole = repoWorkspaceParticipantRole.Search(new SearchWorkspaceParticipantRolesParams { Id = participant.RoleId }, cancellationToken);

                if ((await dataUser).FirstOrDefault() == null)
                {

                }
            }

        }
        finally
        {
            await repoPgLock.ReleaseLock(lockKey, "", cancellationToken);
        }
    }

    public class ReqCreateWorkspace
    {
        public required string Name { get; set; }
        public required ReqCreateWorkspaceParticipant[] Participants { get; set; }
    }

    public class ReqCreateWorkspaceParticipant
    {
        public required string UserId { get; set; }
        public required string RoleId { get; set; }
    }

    public class ReqCreateWorkspaceValidator : AbstractValidator<ReqCreateWorkspace>
    {
        public ReqCreateWorkspaceValidator()
        {
            RuleFor(i => i.Name).NotEmpty().MinimumLength(1);
            RuleForEach(i => i.Participants)
                .NotEmpty()
                .WithMessage("At least one participant is required")
                .ChildRules(participant =>
                {
                    participant.RuleFor(p => p.UserId).NotEmpty().WithMessage("UserId is required");
                    participant.RuleFor(p => p.RoleId).NotEmpty().WithMessage("RoleId is required");
                });
        }
    }
}