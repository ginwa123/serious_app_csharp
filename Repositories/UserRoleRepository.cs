

using App.Db;
using App.Utils;
using Dapper;
using Npgsql;

namespace App.Routes.Users;

public class SearchUserRolesParams
{
    public string Id { get; set; } = string.Empty;
    public string Ids { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public int? Offset { get; set; }
}

public class UserRole
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public interface IUserRoleRepository
{
    Task<IEnumerable<UserRole>> Search(SearchUserRolesParams @params, CancellationToken cancellationToken);
    Task<int> SearchCount(SearchUserRolesParams @params, CancellationToken cancellationToken);
}

public class UserRoleRepository(
    IDbConnectionFactory db,
    IGenerator generator,
    IAppSettings appSettings
) : IUserRoleRepository
{
    private readonly string baseQuerySearchUserRole = @"{0} FROM auth.user_roles WHERE 1 = 1";
    public async Task<IEnumerable<UserRole>> Search(SearchUserRolesParams @params, CancellationToken cancellationToken)
    {
        var fields = "select id, name, created_at, updated_at";
        var query = this.baseQuerySearchUserRole;
        query = SearchUserRoleQueryBuilder(query, @params);

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
        var objs = await conn.QueryAsync<UserRole>(query, @params);
        return objs;
    }

    private static string SearchUserRoleQueryBuilder(string query, SearchUserRolesParams @params)
    {
        if (!string.IsNullOrEmpty(@params.Id))
            query += " AND id = @Id";

        if (@params.Ids?.Length > 0)
            query += " AND id IN @Ids";

        if (!string.IsNullOrEmpty(@params.Name))
            query += " AND name = @Name";

        return query;
    }

    public Task<int> SearchCount(SearchUserRolesParams @params, CancellationToken cancellationToken)
    {
        var fields = "select COUNT(*)";
        var query = this.baseQuerySearchUserRole;
        query = SearchUserRoleQueryBuilder(query, @params);
        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        return conn!.QueryFirstOrDefaultAsync<int>(query, @params);
    }
}