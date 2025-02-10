
using App.Constants;
using App.Db;
using App.Models;
using App.Utils;
using Dapper;
using Npgsql;

namespace App.Repositories;
public class UserRoleAssignee
{
    public string Id { get; set; } = string.Empty;
    public required string UserId { get; set; }
    public required string UserRoleId { get; set; }
    public string? UserRoleName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}


public interface IUserRoleAssigneeRepository
{
    Task<IEnumerable<UserRoleAssignee>> Search(SearchUserRoleAssigneesParams @params, CancellationToken cancellationToken);
    Task<int> SearchCount(SearchUserRoleAssigneesParams @params, CancellationToken cancellationToken);
    Task<UserRoleAssignee?> Create(UserRoleAssignee obj, CancellationToken cancellationToken);
}



public class SearchUserRoleAssigneesParams
{
    public string? Id { get; set; }
    public string[]? Ids { get; set; }
    public string? UserId { get; set; }
    public string[]? UserIds { get; set; }
    public string? UserRoleId { get; set; }
    public string[]? UserRoleIds { get; set; }
    public int? PageSize { get; set; }
    public int? Page { get; set; }
    public int? Offset { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public class UserRoleAssigneeRepository(
IDbConnectionFactory db,
    IGenerator generator,
    IAppSettings appSettings
) : IUserRoleAssigneeRepository
{

    private readonly string baseQuerySearchUserRoleAssignees = @"
    {0} FROM auth.user_role_assignees ura
    JOIN auth.user_roles ur ON ur.id = ura.user_role_id and ur.deleted_at is null
    JOIN auth.users u ON u.id = ura.user_id and u.deleted_at is null
    WHERE 1 = 1";
    public async Task<IEnumerable<UserRoleAssignee>> Search(SearchUserRoleAssigneesParams @params, CancellationToken cancellationToken)
    {
        var fields = "select ura.id, ura.user_id, ura.user_role_id, ur.name as user_role_name, ura.created_at, ura.updated_at";
        var query = this.baseQuerySearchUserRoleAssignees;
        query = SearchUserRoleAssigneesQueryBuilder(query, @params);

        if (@params.PageSize > 0 && @params.Page > 0)
        {
            var offset = (@params.Page - 1) * @params.PageSize;
            query += " LIMIT @PageSize OFFSET @Offset";
            @params.Offset = offset;
        }

        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var UserRoleAssignees = await conn.QueryAsync<UserRoleAssignee>(query, @params);
        return UserRoleAssignees;
    }

    public async Task<int> SearchCount(SearchUserRoleAssigneesParams @params, CancellationToken cancellationToken)
    {
        var fields = "select COUNT(*)";
        var query = this.baseQuerySearchUserRoleAssignees;
        query = SearchUserRoleAssigneesQueryBuilder(query, @params);
        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var count = await conn.QueryFirstOrDefaultAsync<int>(query, @params);
        return count;
    }

    private static string SearchUserRoleAssigneesQueryBuilder(string query, SearchUserRoleAssigneesParams @params)
    {
        if (!string.IsNullOrEmpty(@params.Id))
            query += " AND ura.id = @Id";

        if (@params.Ids?.Length > 0)
            query += " AND ura.id IN @Ids";

        if (!string.IsNullOrEmpty(@params.UserId))
            query += " AND ura.user_id = @UserId";

        if (@params.DeletedAt != null)
            query += " AND Date(ura.deleted_at) = @DeletedAt";
        else if (@params.DeletedAt == null)
            query += " AND ura.deleted_at IS NULL";


        return query;
    }

    public async Task<UserRoleAssignee?> Create(UserRoleAssignee obj, CancellationToken cancellationToken)
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
                SELECT id FROM auth.user_role_assignees
                WHERE id = @Id";
            var dataExist = await conn.QueryFirstOrDefaultAsync<string>(queryCheck, obj);
            if (dataExist == null)
                break;
            attempts++;
            sizeNanoId++;
        }

        if (attempts >= maxAttempts)
            throw new UnhandledException(code: ErrorConstants.ID_NANO_CONFLICT, $"Failed to create {nameof(ApiUrl)}");

        var query = @"
        INSERT INTO auth.user_role_assignees (id, user_id, user_role_id, created_at, updated_at)
        VALUES (@Id, @UserId, @UserRoleId, @CreatedAt, @UpdatedAt)
        RETURNING id, user_id, user_role_id, created_at, updated_at
        ";

        return await conn.QueryFirstOrDefaultAsync<UserRoleAssignee>(query, obj);
    }
}
