
using App.Db;
using App.Utils;
using Dapper;
using Npgsql;

namespace App.Repositories;

public class SearchWorkspaceParticipantRolesParams
{
    public string Id { get; set; } = string.Empty;
    public string Ids { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public int? Offset { get; set; }
}

public class WorkspaceParticipantRole
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public interface IWorkspaceParticipantRoleRepository
{
    Task<IEnumerable<WorkspaceParticipantRole>> Search(SearchWorkspaceParticipantRolesParams @params, CancellationToken cancellationToken);
    Task<int> SearchCount(SearchWorkspaceParticipantRolesParams @params, CancellationToken cancellationToken);
}

public class WorkspaceParticipantRoleRepository(
    IDbConnectionFactory db,
    IGenerator generator,
    IAppSettings appSettings
) : IWorkspaceParticipantRoleRepository
{
    private readonly string baseQuerySearchWorkspaceParticipantRole = @"{0} FROM auth.workspace_participant_roles WHERE 1 = 1";
    public async Task<IEnumerable<WorkspaceParticipantRole>> Search(SearchWorkspaceParticipantRolesParams @params, CancellationToken cancellationToken)
    {
        var fields = "select id, name, created_at, updated_at";
        var query = this.baseQuerySearchWorkspaceParticipantRole;
        query = SearchWorkspaceParticipantRoleQueryBuilder(query, @params);

        query += " ORDER BY created_at DESC";
        if (@params.PageSize > 0 && @params.Page > 0)
        {
            var offset = (@params.Page - 1) * @params.PageSize;
            query += " LIMIT @PageSize OFFSET @Offset";
            @params.Offset = offset;
        }

        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var objs = await conn.QueryAsync<WorkspaceParticipantRole>(query, @params);
        return objs;
    }

    private static string SearchWorkspaceParticipantRoleQueryBuilder(string query, SearchWorkspaceParticipantRolesParams @params)
    {
        if (!string.IsNullOrEmpty(@params.Id))
            query += " AND id = @Id";

        if (@params.Ids?.Length > 0)
            query += " AND id IN @Ids";

        if (!string.IsNullOrEmpty(@params.Name))
            query += " AND name = @Name";

        return query;
    }

    public Task<int> SearchCount(SearchWorkspaceParticipantRolesParams @params, CancellationToken cancellationToken)
    {
        var fields = "select COUNT(DISTINCT id)";
        var query = this.baseQuerySearchWorkspaceParticipantRole;
        query = SearchWorkspaceParticipantRoleQueryBuilder(query, @params);
        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        return conn!.QueryFirstOrDefaultAsync<int>(query, @params);
    }
}