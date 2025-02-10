
using App.Db;
using App.Utils;
using Dapper;
using Npgsql;

namespace App.Repositories;

public class SearchWorkspaceParticipantParams
{
    public string Id { get; set; } = string.Empty;
    public string Ids { get; set; } = string.Empty;
    public string WorkspaceId { get; set; } = string.Empty;
    public string[]? WorkspaceIds { get; set; } = [];
    public string UserId { get; set; } = string.Empty;

    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public int? Offset { get; set; }
}

public class WorkspaceParticipant
{
    public string Id { get; set; } = string.Empty;
    public string WorkspaceId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public interface IWorkspaceParticipantRepository
{
    Task<IEnumerable<WorkspaceParticipant>> Search(SearchWorkspaceParticipantParams @params, CancellationToken cancellationToken);
    Task<int> SearchCount(SearchWorkspaceParticipantParams @params, CancellationToken cancellationToken);
}

public class WorkspaceParticipantRepository(
    IDbConnectionFactory db,
    IGenerator generator,
    IAppSettings appSettings
) : IWorkspaceParticipantRepository
{
    private readonly string baseQuerySearchWorkspaceParticipant = @"{0} FROM auth.workspace_participants awp
    left join auth.users u on awp.user_id = u.id and u.deleted_at is null
     WHERE 1 = 1";
    public async Task<IEnumerable<WorkspaceParticipant>> Search(SearchWorkspaceParticipantParams @params, CancellationToken cancellationToken)
    {
        var fields = "select awp.id, awp.workspace_id, awp.created_at, awp.updated_at, u.id as user_id, u.name as user_name";
        var query = this.baseQuerySearchWorkspaceParticipant;
        query = SearchWorkspaceParticipantQueryBuilder(query, @params);

        query += " ORDER BY awp.created_at DESC";
        if (@params.PageSize > 0 && @params.Page > 0)
        {
            var offset = (@params.Page - 1) * @params.PageSize;
            query += " LIMIT @PageSize OFFSET @Offset";
            @params.Offset = offset;
        }

        query = string.Format(query, fields);
        using var conn = db.CreateConnection();
        await conn!.OpenAsync(cancellationToken);
        var objs = await conn.QueryAsync<WorkspaceParticipant>(query, @params);
        return objs;
    }

    private static string SearchWorkspaceParticipantQueryBuilder(string query, SearchWorkspaceParticipantParams @params)
    {
        if (!string.IsNullOrEmpty(@params.Id))
            query += " AND awp.id = @Id";

        if (@params.Ids?.Length > 0)
            query += " AND awp.id IN @Ids";

        if (!string.IsNullOrEmpty(@params.WorkspaceId))
            query += " AND awp.workspace_id = @WorkspaceId";

        if (@params.WorkspaceIds?.Length > 0)
            query += " AND awp.workspace_id = ANY(@WorkspaceIds)";

        if (!string.IsNullOrEmpty(@params.UserId))
            query += " AND awp.user_id = @UserId";

        return query;
    }

    public Task<int> SearchCount(SearchWorkspaceParticipantParams @params, CancellationToken cancellationToken)
    {
        var fields = "select COUNT(DISTINCT awp.id)";
        var query = this.baseQuerySearchWorkspaceParticipant;
        query = SearchWorkspaceParticipantQueryBuilder(query, @params);
        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        return conn!.QueryFirstOrDefaultAsync<int>(query, @params);
    }
}