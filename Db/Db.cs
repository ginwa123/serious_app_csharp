using Npgsql;

namespace App.Db;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();
}

public class SqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString);
    }
}

