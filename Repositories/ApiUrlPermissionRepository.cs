
using App.Constants;
using App.Db;
using App.Models;
using App.Utils;
using Dapper;
using Npgsql;

namespace App.Repositories;
public class ApiUrlPermission
{
    public string Id { get; set; } = "";
    public required string ApiUrlId { get; set; }
    public required string UserPermissionId { get; set; }
    public string? UserPermissionName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}


public interface IApiUrlPermissionRepository
{
    Task<IEnumerable<ApiUrlPermission>> SearchApiUrlPermissions(SearchApiUrlPermissionsParams @params, CancellationToken cancellationToken);
    Task<int> SearchApiUrlPermissionsCount(SearchApiUrlPermissionsParams @params, CancellationToken cancellationToken);
    Task<ApiUrlPermission?> Create(ApiUrlPermission obj, CancellationToken cancellationToken);
    Task<int> Delete(string Id, CancellationToken cancellationToken);
    Task HardDeleteClearTrash(CancellationToken cancellationToken);
}



public class SearchApiUrlPermissionsParams
{
    public string? Id { get; set; }
    public string[]? Ids { get; set; }
    public string? ApiUrlId { get; set; }
    public string[]? ApiUrlIds { get; set; }
    public string? UserPermissionId { get; set; }
    public string[]? UserPermissionIds { get; set; }
    public int? PageSize { get; set; }
    public int? Page { get; set; }
    public int? Offset { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public class HardDeleteParams
{
    public DateTimeOffset? DeletedAt { get; set; }
}

public class ApiUrlPermissionRepository(
    IDbConnectionFactory db,
    IGenerator generator,
    IAppSettings appSettings) : IApiUrlPermissionRepository
{
    private readonly IDbConnectionFactory db = db;

    private readonly string baseQuerySearchApiUrlPermissions = @"
    {0} FROM auth.api_url_permissions au
    JOIN auth.user_permissions up ON au.user_permission_id = up.id and up.deleted_at is null
    WHERE 1 = 1";
    public async Task<IEnumerable<ApiUrlPermission>> SearchApiUrlPermissions(SearchApiUrlPermissionsParams @params, CancellationToken cancellationToken)
    {
        var fields = "select au.id, au.api_url_id, au.user_permission_id, up.name as user_permission_name, au.created_at, au.updated_at";
        var query = this.baseQuerySearchApiUrlPermissions;
        query = SearchApiUrlPermissionsQueryBuilder(query, @params);

        if (@params.PageSize > 0 && @params.Page > 0)
        {
            var offset = (@params.Page - 1) * @params.PageSize;
            query += " LIMIT @PageSize OFFSET @Offset";
            @params.Offset = offset;
        }

        query = string.Format(query, fields);
        using var conn = db.CreateConnection();
        await conn!.OpenAsync(cancellationToken);
        var ApiUrlPermissions = await conn.QueryAsync<ApiUrlPermission>(query, @params);
        return ApiUrlPermissions;
    }

    public async Task<int> SearchApiUrlPermissionsCount(SearchApiUrlPermissionsParams @params, CancellationToken cancellationToken)
    {
        var fields = "select COUNT(*)";
        var query = this.baseQuerySearchApiUrlPermissions;
        query = SearchApiUrlPermissionsQueryBuilder(query, @params);
        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var count = await conn.QueryFirstOrDefaultAsync<int>(query, @params);
        return count;
    }

    private static string SearchApiUrlPermissionsQueryBuilder(string query, SearchApiUrlPermissionsParams @params)
    {
        if (!string.IsNullOrEmpty(@params.Id))
            query += " AND au.id = @Id";

        if (@params.Ids?.Length > 0)
            query += " AND au.id IN @Ids";

        if (!string.IsNullOrEmpty(@params.ApiUrlId))
            query += " AND au.api_url_id = @ApiUrlId";

        if (@params.ApiUrlIds?.Length > 0)
            query += " AND au.api_url_id  = ANY(@ApiUrlIds)";

        if (@params.DeletedAt != null)
            query += " AND Date(au.deleted_at) = @DeletedAt";
        else if (@params.DeletedAt == null)
            query += " AND au.deleted_at IS NULL";


        return query;
    }

    public async Task<ApiUrlPermission?> Create(ApiUrlPermission obj, CancellationToken cancellationToken)
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
                SELECT id FROM auth.api_url_permissions
                WHERE id = @Id";
            var dataExist = await conn.QueryFirstOrDefaultAsync<string>(queryCheck, obj);
            if (dataExist == null)
                break;
            attempts++;
            sizeNanoId++;
        }

        if (attempts >= maxAttempts)
            throw new UnhandledException(code: ErrorConstants.ID_NANO_CONFLICT, $"Failed to create {nameof(ApiUrlPermission)}");

        var query = @"
        INSERT INTO auth.api_url_permissions (id, api_url_id, user_permission_id, created_at, updated_at)
        VALUES (@Id, @ApiUrlId, @UserPermissionId, @CreatedAt, @UpdatedAt)
        RETURNING id, api_url_id, user_permission_id, created_at, updated_at";
        return await conn.QueryFirstOrDefaultAsync<ApiUrlPermission>(query, obj);
    }

    public async Task<int> Delete(string Id, CancellationToken cancellationToken)
    {
        var query = @"
        UPDATE auth.api_url_permissions
        SET deleted_at = @DeletedAt
        WHERE id = @Id";

        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        return await conn.ExecuteScalarAsync<int>(query, new
        {
            Id,
            DeletedAt = DateTime.UtcNow
        });
    }

    public async Task HardDeleteClearTrash(CancellationToken cancellationToken)
    {
        var query = @"
        DELETE FROM auth.api_url_permissions
        WHERE 1 = 1 and deleted_at is not null";

        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        await conn.ExecuteAsync(query);
    }
}
