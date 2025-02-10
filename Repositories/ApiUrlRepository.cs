using App.Constants;
using App.Db;
using App.Models;
using App.Utils;
using Dapper;
using Npgsql;

namespace App.Repositories;
public class ApiUrl
{
    public string Id { get; set; } = "";
    public required string Url { get; set; }
    public required string Method { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}


public interface IApiUrlRepository
{
    Task<IEnumerable<ApiUrl>> Search(SearchApiUrlsParams @params, CancellationToken cancellationToken);
    Task<int> SearchCount(SearchApiUrlsParams @params, CancellationToken cancellationToken);
    Task<ApiUrl?> Create(ApiUrl obj, CancellationToken cancellationToken);
    Task<int> Delete(string Id, CancellationToken cancellationToken);
    Task<(ApiUrl?, int)> Update(ApiUrl obj, CancellationToken cancellationToken);
    Task HardDeleteClearTrash(CancellationToken cancellationToken);
}

public class SearchApiUrlsParams
{
    public string? Id { get; set; }
    public string[]? Ids { get; set; }
    public string? Url { get; set; }
    public string[]? Urls { get; set; }
    public string? Method { get; set; }
    public int? PageSize { get; set; }
    public int? Page { get; set; }
    public int? Offset { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public class ApiUrlRepository(
    IDbConnectionFactory db,
    IGenerator generator,
    IAppSettings appSettings
) : IApiUrlRepository
{
    private readonly IDbConnectionFactory db = db;

    private readonly string baseQuerySearchApiUrls = @"{0} FROM auth.api_urls WHERE 1 = 1";
    public async Task<IEnumerable<ApiUrl>> Search(SearchApiUrlsParams @params, CancellationToken cancellationToken)
    {
        var fields = "select id, url, method, created_at, updated_at";
        var query = this.baseQuerySearchApiUrls;
        query = SearchApiUrlsQueryBuilder(query, @params);

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
        var ApiUrls = await conn.QueryAsync<ApiUrl>(query, @params);
        return ApiUrls;
    }

    public async Task<int> SearchCount(SearchApiUrlsParams @params, CancellationToken cancellationToken)
    {
        var fields = "select COUNT(*)";
        var query = this.baseQuerySearchApiUrls;
        query = SearchApiUrlsQueryBuilder(query, @params);
        query = string.Format(query, fields);
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var count = await conn.QueryFirstOrDefaultAsync<int>(query, @params);
        return count;
    }

    private static string SearchApiUrlsQueryBuilder(string query, SearchApiUrlsParams @params)
    {
        if (!string.IsNullOrEmpty(@params.Id))
            query += " AND id = @Id";

        if (@params.Ids?.Length > 0)
            query += " AND id IN @Ids";

        if (!string.IsNullOrEmpty(@params.Url))
            query += " AND url = @Url";

        if (!string.IsNullOrEmpty(@params.Method))
            query += " AND method = @Method";

        if (@params.Urls?.Length > 0)
            query += " AND url IN @Urls";

        if (@params.DeletedAt != null)
            query += " AND Date(deleted_at) = @DeletedAt";
        else if (@params.DeletedAt == null)
            query += " AND deleted_at IS NULL";


        return query;
    }

    public class CreateApiUrlParams
    {
        public required string Id { get; set; }
        public required string Url { get; set; }
        public required string Method { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public async Task<ApiUrl?> Create(ApiUrl obj, CancellationToken cancellationToken)
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
                SELECT id FROM auth.api_urls
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
        INSERT INTO auth.api_urls (id, url, method, created_at, updated_at)
        VALUES (@Id, @Url, @Method, @CreatedAt, @UpdatedAt)
        RETURNING id, url, method, created_at, updated_at";

        return await conn.QueryFirstOrDefaultAsync<ApiUrl>(query, obj);
    }

    public async Task<(ApiUrl?, int)> Update(ApiUrl obj, CancellationToken cancellationToken)
    {
        var query = @"
        UPDATE auth.api_urls
        SET url = @Url, method = @Method, updated_at = @UpdatedAt
        WHERE id = @Id
        RETURNING id, url, method, created_at, updated_at";

        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var result = await conn.QueryAsync<ApiUrl>(query, obj);
        var updatedObj = result.FirstOrDefault();
        var rowsAffected = result.Count();

        return (updatedObj, rowsAffected);
    }

    public async Task<int> Delete(string Id, CancellationToken cancellationToken)
    {
        var query = @"
        UPDATE auth.api_urls
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
        DELETE FROM auth.api_urls
        WHERE 1 = 1 and deleted_at is not null";

        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        await conn.ExecuteAsync(query);
    }
}
