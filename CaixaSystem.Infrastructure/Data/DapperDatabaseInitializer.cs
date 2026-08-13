namespace CaixaSystem.Infrastructure.Data;

using Dapper;
using Microsoft.Data.SqlClient;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

public sealed class DapperDatabaseInitializer : IDatabaseInitializer
{
    private readonly string _connectionString;

    public DapperDatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        var connection = (SqlConnection)connectionFactory.CreateConnection();
        _connectionString = connection.ConnectionString;
        connection.Dispose();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        await using (var master = new SqlConnection(builder.ConnectionString))
        {
            const string createDatabase = """
                IF DB_ID(@DatabaseName) IS NULL
                BEGIN
                    DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@DatabaseName);
                    EXEC sys.sp_executesql @sql;
                END;
                """;
            await master.ExecuteAsync(new CommandDefinition(createDatabase, new { DatabaseName = databaseName }, cancellationToken: cancellationToken));
        }

        await using var connection = new SqlConnection(_connectionString);
        const string schema = """
            IF OBJECT_ID(N'dbo.Transactions', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Transactions (
                    Id uniqueidentifier NOT NULL CONSTRAINT PK_Transactions PRIMARY KEY,
                    Type int NOT NULL,
                    Amount decimal(18,2) NOT NULL,
                    Description nvarchar(500) NOT NULL,
                    CreatedAt datetime2 NOT NULL
                );
                CREATE INDEX IX_Transaction_CreatedAt ON dbo.Transactions(CreatedAt);
            END;

            IF OBJECT_ID(N'dbo.DailyBalances', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.DailyBalances (
                    Id uniqueidentifier NOT NULL CONSTRAINT PK_DailyBalances PRIMARY KEY,
                    Date date NOT NULL,
                    TotalCredits decimal(18,2) NOT NULL,
                    TotalDebits decimal(18,2) NOT NULL,
                    Balance decimal(18,2) NOT NULL,
                    PreviousDayBalance decimal(18,2) NOT NULL,
                    ConsolidatedBalance decimal(18,2) NOT NULL,
                    ConsolidatedAt datetime2 NOT NULL
                );
                CREATE UNIQUE INDEX IX_DailyBalance_Date ON dbo.DailyBalances(Date);
            END;
            """;
        await connection.ExecuteAsync(new CommandDefinition(schema, cancellationToken: cancellationToken));
    }
}
