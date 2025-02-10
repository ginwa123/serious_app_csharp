using System.Collections.Concurrent;
using App.Db;
using Dapper;
using Npgsql;

namespace App.Repositories;

public interface IPgLockRepository
{
    Task<bool> GetLock(string key, string ownerId, TimeSpan timeout, CancellationToken cancellationToken);
    Task<bool> ReleaseLock(string lockKey, string ownerId, CancellationToken cancellationToken);
    Task<bool> ReleaseAllLockAsync(CancellationToken cancellationToken);
    Task<IEnumerable<PgLock>> Search(CancellationToken cancellationToken);
}

public class PgLock
{
    public required string LockKey { get; set; }
    public DateTimeOffset LockedAt { get; set; }
    public string? OwnerId { get; set; }
}

public class PgLockRepository(IDbConnectionFactory db, ILogger<PgLockRepository> logger) : IPgLockRepository
{
    private readonly IDbConnectionFactory db = db;
    private static readonly object objGetLock = new();

    public async Task<bool> ReleaseLock(string lockKey, string ownerId, CancellationToken cancellationToken)
    {
        const string query = @"
            DELETE FROM public.locks
            WHERE lock_key = @lockKey AND owner_id = @ownerId;";

        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var rowsAffected = await conn.ExecuteAsync(query, new { lockKey, ownerId });
        return rowsAffected > 0; // True if lock was released
    }

    public async Task<bool> GetLock(string key, string ownerId, TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            const string acquireQuery = @"
            INSERT INTO public.locks (lock_key, owner_id, locked_at)
            VALUES (@lockKey, @ownerId, now())
            ON CONFLICT (lock_key) DO NOTHING;";

            const string checkQuery = @"
            SELECT owner_id
            FROM public.locks
            WHERE lock_key = @lockKey
            FOR UPDATE;"; // Lock the row for update
            var start = DateTime.UtcNow;
            using var conn = db.CreateConnection() as NpgsqlConnection;
            await conn!.OpenAsync(cancellationToken);
            while (DateTime.UtcNow - start < timeout)
            {
                lock (objGetLock) // Keep the lock for thread safety within the process
                {
                    using var transaction = conn.BeginTransaction(); // Start a transaction
                    try
                    {
                        // Attempt to acquire the lock
                        var rowsAffected = conn.Execute(acquireQuery, new { lockKey = key, ownerId }, transaction);

                        if (rowsAffected > 0)
                        {
                            logger.LogInformation("Lock acquired for key: {Key}", key);
                            transaction.Commit(); // Commit the transaction if the lock is acquired
                            return true;
                        }

                        // Check if the lock is already held by another owner
                        var currentOwner = conn.QueryFirstOrDefault<string>(checkQuery, new { lockKey = key }, transaction);

                        if (currentOwner == ownerId)
                        {
                            // Lock is already held by the same owner
                            transaction.Commit();
                            return true;
                        }

                        // Lock is held by someone else
                        logger.LogInformation("Lock already held for key: {Key}. Retrying...", key);
                        transaction.Rollback(); // Rollback the transaction
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Rollback on error
                        logger.LogError(ex, "Error acquiring lock for key: {Key}", key);
                        throw; // Re-throw the exception
                    }
                }

                await Task.Delay(100, cancellationToken); // Wait before retrying
            }


            return false;
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    public async Task<bool> ReleaseAllLockAsync(CancellationToken cancellationToken)
    {
        var query = "DELETE FROM public.locks";
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        var rowsAffected = await conn.ExecuteAsync(query);

        return rowsAffected > 0;
    }

    public async Task<IEnumerable<PgLock>> Search(CancellationToken cancellationToken)
    {
        var query = "SELECT lock_key, locked_at, owner_id FROM public.locks";
        using var conn = db.CreateConnection() as NpgsqlConnection;
        await conn!.OpenAsync(cancellationToken);
        return await conn.QueryAsync<PgLock>(query);
    }
}

