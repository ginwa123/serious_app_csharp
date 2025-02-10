

using App.Constants;
using App.Db;
using App.Models;
using App.Utils;
using Dapper;
using Npgsql;

namespace App.Repositories;

public class SearchWorkspacesParams
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? UserId { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public int? Offset { get; set; }
}

public class Workspace
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public interface IWorkspaceRepository
{
    Task<IEnumerable<Workspace>> Search(SearchWorkspacesParams @params, CancellationToken cancellationToken);
    Task<int> SearchCount(SearchWorkspacesParams @params, CancellationToken cancellationToken);
    Task<Workspace?> Create(Workspace obj, CancellationToken cancellationToken);
    Task<int> Delete(string Id, CancellationToken cancellationToken);
    Task<(Workspace?, int)> Update(Workspace obj, CancellationToken cancellationToken);
}

public class WorkspaceRepository(
    IDbConnectionFactory db,
    IGenerator generator,
    IAppSettings appSettings
) : IWorkspaceRepository
{
    private readonly string baseQuerySearchWorkspaces = @"
    {0} from auth.workspaces aw
    left join auth.workspace_participants awp on awp.workspace_id = aw.id and awp.deleted_at is null
    where 1 = 1";
    public async Task<IEnumerable<Workspace>> Search(SearchWorkspacesParams @params, CancellationToken cancellationToken)
    {

        var fields = "select distinct aw.id, aw.name, aw.created_at, aw.updated_at";
        var query = this.baseQuerySearchWorkspaces;
        query = SearchWorkspacesQueryBuilder(query, @params);

        query += " ORDER BY aw.created_at DESC";
        if (@params.PageSize > 0 && @params.Page > 0)
        {
            var offset = (@params.Page - 1) * @params.PageSize;
            query += " LIMIT @PageSize OFFSET @Offset";
            @params.Offset = offset;
        }

        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var Workspaces = await conn.QueryAsync<Workspace>(query, @params);
        return Workspaces;
    }

    private static string SearchWorkspacesQueryBuilder(string query, SearchWorkspacesParams @params)
    {
        if (!string.IsNullOrEmpty(@params.Name))
            query += " and aw.name like @Name";
        if (!string.IsNullOrEmpty(@params.Id))
            query += " and aw.id = @Id";
        if (!string.IsNullOrEmpty(@params.UserId))
            query += " and awp.user_id = @UserId";
        return query;
    }

    public async Task<int> SearchCount(SearchWorkspacesParams @params, CancellationToken cancellationToken)
    {
        var fields = "select COUNT(DISTINCT aw.id)";
        var query = this.baseQuerySearchWorkspaces;
        query = SearchWorkspacesQueryBuilder(query, @params);
        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var count = await conn.QueryFirstOrDefaultAsync<int>(query, @params);
        return count;
    }

    public async Task<Workspace?> Create(Workspace obj, CancellationToken cancellationToken)
    {
        var attempts = 0;
        var maxAttempts = appSettings.SeriousApp.DB.RetryCreateCount;
        var sizeNanoId = 5;
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        while (attempts <= maxAttempts)
        {
            obj.Id = await generator.GenerateNanoId(sizeNanoId);
            var queryCheck = @"
                SELECT id FROM auth.workspaces
                WHERE id = @Id";
            var dataExist = await conn.QueryFirstOrDefaultAsync<string>(queryCheck, obj);
            if (dataExist == null)
                break;
            attempts++;
            sizeNanoId++;
        }

        if (attempts >= maxAttempts)
            throw new UnhandledException(code: ErrorConstants.ID_NANO_CONFLICT, $"Failed to create {nameof(Workspace)}");

        var query = @"
        INSERT INTO auth.workspaces (id, name, created_at, updated_at)
        VALUES (@Id, @Name , @CreatedAt, @UpdatedAt)
        RETURNING id, name, created_at, updated_at";

        return await conn.QueryFirstOrDefaultAsync<Workspace>(query, obj);
    }

    public async Task<int> Delete(string Id, CancellationToken cancellationToken)
    {
        var query = @"
        UPDATE auth.workspaces
        SET deleted_at = @DeletedAt
        WHERE id = @Id";

        using var conn = db.CreateConnection();
        await conn!.OpenAsync(cancellationToken);
        return await conn.ExecuteScalarAsync<int>(query, new
        {
            Id,
            DeletedAt = DateTime.UtcNow
        });
    }

    public async Task<(Workspace?, int)> Update(Workspace obj, CancellationToken cancellationToken)
    {
        var query = @"
        UPDATE auth.workspaces
        SET name = @Name, updated_at = @UpdatedAt
        WHERE id = @Id
        RETURNING id, name, created_at, updated_at";

        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var result = await conn.QueryAsync<Workspace>(query, obj);
        var updatedObj = result.FirstOrDefault();
        var rowsAffected = result.Count();

        return (updatedObj, rowsAffected);
    }
}