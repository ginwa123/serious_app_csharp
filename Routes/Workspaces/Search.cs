


using App.Constants;
using App.Extensions;
using App.Models;
using App.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace App.Routes.Workspaces;


public static class SearchWorkspaceRoute
{
    public static async Task<Response<ResSearchWorkspaces>> Do(
        HttpContext context,
        [FromServices] IWorkspaceRepository repoWorkspace,
        [FromQuery(Name = "id")] string? id,
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "page_size")] int? pageSize,
        [FromQuery(Name = "name")] string? name,
        [FromQuery(Name = "user_id")] string? userId,
        [FromQuery(Name = "list_joins")] string? listJoins,
        [FromServices] IWorkspaceParticipantRepository repoWorkspaceParticipant,
        CancellationToken cancellationToken
        )
    {
        var logId = context.Items[LogConstants.LOG]?.ToString();
        var @params = new SearchWorkspacesParams()
        {
            Id = id,
            Page = page,
            PageSize = pageSize,
            Name = name,
            UserId = userId
        };
        var data = repoWorkspace.Search(@params, cancellationToken);
        var dataCount = repoWorkspace.SearchCount(@params, cancellationToken);

        var listParticipants = new List<ResSearchWorkspaceDataParticipant>();
        if (listJoins != null)
        {
            var listJoinsArray = listJoins.Split(",");
            foreach (var item in listJoinsArray)
            {
                if (item == "participants")
                {
                    var @paramParticipant = new SearchWorkspaceParticipantParams()
                    {
                        WorkspaceIds = (await data)?.Select(x => x.Id).ToArray()
                    };
                    var workspaceParticipant = await repoWorkspaceParticipant.Search(paramParticipant, cancellationToken);

                    foreach (var participant in workspaceParticipant)
                    {
                        listParticipants.Add(new ResSearchWorkspaceDataParticipant
                        {
                            WorkspaceId = participant?.WorkspaceId ?? string.Empty,
                            Id = participant?.Id ?? string.Empty,
                            UserId = participant?.UserId ?? string.Empty,
                            UserName = participant?.UserName ?? string.Empty
                        });
                    }
                }
            }
        }
        return new Response<ResSearchWorkspaces>()
        {
            Message = BasicConstants.SUCCCESS,
            Data = new ResSearchWorkspaces
            {
                Items = (await data).Select(x =>
                {
                    return new ResSearchWorkspaceData
                    {
                        Id = x?.Id ?? string.Empty,
                        Name = x?.Name ?? string.Empty,
                        CreatedAt = x?.CreatedAt.ToDefaultString() ?? string.Empty,
                        UpdatedAt = x?.UpdatedAt.ToDefaultString() ?? string.Empty,
                        Participants = [.. listParticipants.Where(p => p.WorkspaceId == x?.Id)]
                    };
                }).ToArray() ?? [],
                Meta = new ResponseMeta
                {
                    TotalRecord = await dataCount,
                    PageSize = @params.PageSize ?? await dataCount,
                    Page = @params.Page ?? 1,
                }
            },
            Log = logId!
        };
    }

    public class ResSearchWorkspaces
    {
        public required ResSearchWorkspaceData[] Items { get; set; } = [];
        public required ResponseMeta Meta { get; set; }
    }

    public class ResSearchWorkspaceData
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string CreatedAt { get; set; }
        public required string UpdatedAt { get; set; }
        public ResSearchWorkspaceDataParticipant[] Participants { get; set; } = [];
    }

    public class ResSearchWorkspaceDataParticipant
    {
        public string? WorkspaceId { get; set; }
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }
}