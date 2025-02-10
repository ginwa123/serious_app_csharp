

using App.Db;
using App.Utils;
using Dapper;
using Npgsql;

namespace App.Repositories;


public interface IUserRolePermissionRepository
{
    Task<IEnumerable<UserRolePermission>> Search(SearchUserRolerPermissionParams @params, CancellationToken cancellationToken);

}

public class SearchUserRolerPermissionParams
{
    public string? Id { get; set; }
    public string? UserRoleId { get; set; }
    public string[]? UserRoleIds { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public int? Offset { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

}

public class UserRolePermission
{
    public string Id { get; set; } = string.Empty;
    public string UserRoleId { get; set; } = string.Empty;
    public string UserPermissionId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class UserRolePermissionRepository(
     IDbConnectionFactory db,
    IGenerator generator,
    IAppSettings appSettings
) : IUserRolePermissionRepository
{
    private string baseQuerySearchUserRolePermissions = @"{0} FROM auth.user_role_permissions where 1 = 1 ";

    public async Task<IEnumerable<UserRolePermission>> Search(SearchUserRolerPermissionParams @params, CancellationToken cancellationToken)
    {
        var fields = "select id, user_role_id, user_permission_id, created_at, updated_at";
        var query = this.baseQuerySearchUserRolePermissions;
        query = SearchUserRolePermissionsQueryBuilder(query, @params);

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
        var UserRolePermissions = await conn.QueryAsync<UserRolePermission>(query, @params);
        return UserRolePermissions;
    }

    private static string SearchUserRolePermissionsQueryBuilder(string query, SearchUserRolerPermissionParams @params)
    {
        if (!string.IsNullOrEmpty(@params.Id))
            query += " AND id = @Id";

        if (!string.IsNullOrEmpty(@params.UserRoleId))
            query += " AND user_role_id = @UserRoleId";

        if (@params.UserRoleIds?.Length > 0)
            query += " AND user_role_id = ANY(@UserRoleIds)";

        if (@params.DeletedAt != null)
            query += " AND Date(deleted_at) = @DeletedAt";
        else if (@params.DeletedAt == null)
            query += " AND deleted_at IS NULL";

        return query;

    }
}