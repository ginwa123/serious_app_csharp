using App.Constants;
using App.Db;
using App.Models;
using App.Utils;
using Dapper;
using Npgsql;
namespace App.Repositories;


public class UserPermission
{
    public string Id { get; set; } = "";
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public static string[] MandatorUserPermissions = ["can_readall", "can_createall", "can_updateall", "can_deleteall"];

}


public interface IUserPermissionRepository
{
    Task<UserPermission?> Create(UserPermission obj, CancellationToken cancellationToken);
    Task<IEnumerable<UserPermission>> Search(SearchUserPermissionsParams @params, CancellationToken cancellationToken);
    Task<int> SearchCount(SearchUserPermissionsParams @params, CancellationToken cancellationToken);
    Task<int> Delete(string Id, CancellationToken cancellationToken);

}

public class SearchUserPermissionsParams
{
    public string? Id { get; set; }
    public string[]? Ids { get; set; }
    public string? Name { get; set; }
    public string? NameLike { get; set; }
    public string[]? Names { get; set; }

    public int? PageSize { get; set; }
    public int? Page { get; set; }
    public int? Offset { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public class UserPermissionRepository(
    IDbConnectionFactory db,
    IGenerator generator,
    IAppSettings appSettings
    ) : IUserPermissionRepository
{
    private readonly IDbConnectionFactory db = db;

    private readonly string baseQuerySearchUserPermissions = @"{0} FROM auth.user_permissions
    WHERE 1 = 1";
    public async Task<IEnumerable<UserPermission>> Search(SearchUserPermissionsParams @params, CancellationToken cancellationToken)
    {
        var fields = "select id, name, created_at, updated_at";
        var query = this.baseQuerySearchUserPermissions;
        query = SearchQueryBuilder(query, @params);

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
        var UserPermissions = await conn.QueryAsync<UserPermission>(query, @params);
        return UserPermissions;
    }

    public async Task<int> SearchCount(SearchUserPermissionsParams @params, CancellationToken cancellationToken)
    {
        var fields = "select COUNT(*)";
        var query = this.baseQuerySearchUserPermissions;
        query = SearchQueryBuilder(query, @params);
        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var count = await conn.QueryFirstOrDefaultAsync<int>(query, @params);
        return count;
    }

    private static string SearchQueryBuilder(string query, SearchUserPermissionsParams @params)
    {
        if (!string.IsNullOrEmpty(@params.Id))
            query += " AND id = @Id";

        if (@params.Ids?.Length > 0)
            query += " AND id IN @Ids";

        if (!string.IsNullOrEmpty(@params.Name))
            query += " AND name = @Name";

        if (!string.IsNullOrEmpty(@params.NameLike))
        {
            query += " AND name LIKE @NameLike";
            @params.NameLike = "%" + @params.NameLike + "%";
        }

        if (@params.DeletedAt != null)
            query += " AND Date(deleted_at) = @DeletedAt";
        else if (@params.DeletedAt == null)
            query += " AND deleted_at IS NULL";


        return query;
    }

    public class CreateUserPermissionParams
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public async Task<UserPermission?> Create(UserPermission obj, CancellationToken cancellationToken)
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
                SELECT id FROM auth.user_permissions
                WHERE id = @Id";
            var dataExist = await conn.QueryFirstOrDefaultAsync<string>(queryCheck, obj);
            if (dataExist == null)
                break;
            attempts++;
            sizeNanoId++;
        }

        if (attempts >= maxAttempts)
            throw new UnhandledException(code: ErrorConstants.ID_NANO_CONFLICT, $"Failed to create {nameof(UserPermissionRepository)}");

        var query = @"
        INSERT INTO auth.user_permissions (id, name, created_at, updated_at)
        VALUES (@Id, @Name, @CreatedAt, @UpdatedAt)
        RETURNING id, name, created_at, updated_at";

        return await conn.QueryFirstOrDefaultAsync<UserPermission>(query, obj);
    }

    public async Task<(UserPermission?, int)> Update(UserPermission obj)
    {
        var query = @"
        UPDATE auth.user_permissions
        SET name = @Name,  updated_at = @UpdatedAt
        WHERE id = @Id
        RETURNING id, name, created_at, updated_at";

        using var conn = db.CreateConnection();
        var result = await conn.QueryAsync<UserPermission>(query, obj);
        var updatedObj = result.FirstOrDefault();
        var rowsAffected = result.Count();

        return (updatedObj, rowsAffected);
    }

    public async Task<int> Delete(string Id)
    {
        var query = @"
        UPDATE auth.user_permissions
        SET deleted_at = @DeletedAt
        WHERE id = @Id";

        using var conn = db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(query, new
        {
            Id,
            DeletedAt = DateTime.UtcNow
        });
    }

    public async Task<int> Delete(string Id, CancellationToken cancellationToken)
    {
        var query = @"
        UPDATE auth.user_permissions
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
}
