namespace CaixaSystem.Infrastructure.Data;

using System.Data.Common;
using Microsoft.Data.SqlClient;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new ArgumentException("A connection string is required.", nameof(connectionString));
    }

    public DbConnection CreateConnection() => new SqlConnection(_connectionString);
}
