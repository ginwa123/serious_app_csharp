


using App.Constants;
using App.Extensions;
using App.Models;
using App.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.Workspaces;


public static class GetWorkspaceByIdRoute
{
    public static async Task<Response<ResGetWorkspaceByIdData>> Do(
        HttpContext context,
        [FromRoute(Name = "id")] string id,
        [FromQuery(Name = "list_joins")] string? listJoins,
        [FromServices] IWorkspaceRepository repoWorkspace,
        [FromServices] IWorkspaceParticipantRepository repoWorkspaceParticipant,
        CancellationToken cancellationToken
        )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var @params = new SearchWorkspacesParams()
        {
            Id = id
        };
        var data = (await SearchWorkspaceRoute.Do(
            context: context,
            repoWorkspace: repoWorkspace,
            id: id,
            page: null,
            pageSize: null,
            name: null,
            userId: null,
            listJoins: listJoins,
            cancellationToken: cancellationToken,
            repoWorkspaceParticipant: repoWorkspaceParticipant
            )).Data?.Items.FirstOrDefault();
        if (data == null)
            throw new NotFoundException(code: ErrorConstants.API_GET_WORKSPACE_BY_ID_ERROR_NOT_FOUND, "Workspace not found");

        return new Response<ResGetWorkspaceByIdData>()
        {
            Message = BasicConstants.SUCCCESS,
            Data = new ResGetWorkspaceByIdData()
            {
                Id = data?.Id ?? string.Empty,
                Name = data?.Name ?? string.Empty,
                CreatedAt = data?.CreatedAt ?? string.Empty,
                UpdatedAt = data?.UpdatedAt ?? string.Empty,
                Participants = data?.Participants.Select(p => new ResGetWorkspaceByIdDataParticipant()
                {
                    Id = p?.Id ?? string.Empty,
                    UserId = p?.UserId ?? string.Empty,
                    UserName = p?.UserName ?? string.Empty,
                    WorkspaceId = p?.WorkspaceId ?? string.Empty,
                    CreatedAt = string.Empty,
                    UpdatedAt = string.Empty
                }).ToArray() ?? []
                // Participants = data.
            },
            Log = logId!
        };
    }

    public class ResGetWorkspaceByIdData
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string CreatedAt { get; set; }
        public required string UpdatedAt { get; set; }
        public ResGetWorkspaceByIdDataParticipant[] Participants { get; set; } = [];

    }

    public class ResGetWorkspaceByIdDataParticipant
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? WorkspaceId { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
    }
}