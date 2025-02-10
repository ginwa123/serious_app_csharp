
using App.Constants;
using App.Db;
using App.Models;
using App.Utils;
using Dapper;
using Npgsql;
namespace App.Repositories;


public class User
{
    public string Id { get; set; } = string.Empty;
    public required string Name { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public UserRoleAssignee[]? UserRoleAssignees { get; set; }
}

public class SearchUsersParams()
{
    public string? Id { get; set; }
    public List<string>? Ids { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
    public string? UsernameLike { get; set; }
    public int? PageSize { get; set; }
    public int? Page { get; set; }
    public int? Offset { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public interface IUserRepository
{
    Task<int> SearchCount(SearchUsersParams @params, CancellationToken cancellationToken);
    Task<IEnumerable<User>> Search(SearchUsersParams @params, CancellationToken cancellcationToken);
    Task<User?> Create(User obj, CancellationToken cancellationToken);
    Task<(User?, int)> Update(User obj, CancellationToken cancellationToken);
    Task<int> Delete(string Id, CancellationToken cancellationToken);

}

public class UserRepository(
    IDbConnectionFactory db,
    IGenerator generator,
    IAppSettings appSettings
) : IUserRepository
{
    private readonly string baseQuerySearchUsers = @"{0} FROM auth.users WHERE 1 = 1";
    public async Task<IEnumerable<User>> Search(SearchUsersParams @params, CancellationToken token)
    {
        var fields = "select id, username, name, password, created_at, updated_at";
        var query = this.baseQuerySearchUsers;
        query = SearchQueryBuilder(query, @params);

        if (@params.PageSize > 0 && @params.Page > 0)
        {
            var offset = (@params.Page - 1) * @params.PageSize;
            query += " LIMIT @PageSize OFFSET @Offset";
            @params.Offset = offset;
        }

        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(token);
        var users = await conn.QueryAsync<User>(query, @params);
        return users;
    }

    public async Task<int> SearchCount(SearchUsersParams @params, CancellationToken token)
    {
        var fields = "select COUNT(*)";
        var query = this.baseQuerySearchUsers;
        query = SearchQueryBuilder(query, @params);
        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(token);

        var count = await conn.QueryFirstOrDefaultAsync<int>(query, @params);
        return count;
    }

    private static string SearchQueryBuilder(string query, SearchUsersParams @params)
    {
        if (!string.IsNullOrEmpty(@params.Id))
            query += " AND id = @Id";
        if (@params.Ids?.Count > 0)
            query += " AND id IN @Ids";
        if (!string.IsNullOrEmpty(@params.Username))
            query += " AND username = @Username";
        if (!string.IsNullOrEmpty(@params.Name))
            query += " AND name = @Name";
        if (!string.IsNullOrEmpty(@params.UsernameLike))
            query += " AND username LIKE @UsernameLike";
        if (@params.DeletedAt != null)
            query += " AND Date(deleted_at) = @DeletedAt";
        else if (@params.DeletedAt == null)
            query += " AND deleted_at IS NULL";

        return query;
    }

    public async Task<User?> Create(User obj, CancellationToken cancellationToken)
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
                SELECT id FROM auth.users
                WHERE id = @Id";
            var dataExist = await conn.QueryFirstOrDefaultAsync<string>(queryCheck, obj);
            if (dataExist == null)
                break;
            attempts++;
            sizeNanoId++;
        }

        if (attempts >= maxAttempts)
            throw new UnhandledException(code: ErrorConstants.ID_NANO_CONFLICT, $"Failed to create {nameof(User)}");

        var query = @"INSERT INTO auth.users (id, name, username, password, created_at, updated_at)
                    VALUES (@Id, @Name, @Username, @Password, @CreatedAt, @UpdatedAt)
                    RETURNING id, name, username, password, created_at, updated_at";

        return await conn!.QueryFirstOrDefaultAsync<User>(query, obj);
    }

    public async Task<(User?, int)> Update(User obj, CancellationToken cancellationToken)
    {
        var query = @"
        UPDATE auth.users
        SET name = @Name, username = @Username, password = @Password, updated_at = @UpdatedAt
        WHERE id = @Id
        RETURNING id, name, username, password, created_at, updated_at";
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var result = await conn.QueryAsync<User>(query, obj);
        var updatedObj = result.FirstOrDefault();
        var rowsAffected = result.Count();

        return (updatedObj, rowsAffected);
    }

    public async Task<int> Delete(string Id, CancellationToken cancellationToken)
    {
        var query = @"
        UPDATE auth.users
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

