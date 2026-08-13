namespace CaixaSystem.Infrastructure.Repositories;

using CaixaSystem.Domain.Entities;
using CaixaSystem.Domain.Enums;
using CaixaSystem.Domain.Repositories;
using CaixaSystem.Infrastructure.Data;
using Dapper;

public sealed class DapperTransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    public DapperTransactionRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        const string sql = "INSERT INTO dbo.Transactions (Id, Type, Amount, Description, CreatedAt) VALUES (@Id, @Type, @Amount, @Description, @CreatedAt);";
        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            transaction.Id,
            Type = (int)transaction.Type,
            Amount = transaction.Amount.Amount,
            transaction.Description,
            transaction.CreatedAt
        }, cancellationToken: cancellationToken));
    }

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        QuerySingleAsync("SELECT Id, Type, Amount, Description, CreatedAt FROM dbo.Transactions WHERE Id = @Id;", new { Id = id }, cancellationToken);

    public Task<IEnumerable<Transaction>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) =>
        QueryAsync("SELECT Id, Type, Amount, Description, CreatedAt FROM dbo.Transactions WHERE CreatedAt >= @Start AND CreatedAt < @End ORDER BY CreatedAt;",
            new { Start = date.ToDateTime(TimeOnly.MinValue), End = date.AddDays(1).ToDateTime(TimeOnly.MinValue) }, cancellationToken);

    public Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        QueryAsync("SELECT Id, Type, Amount, Description, CreatedAt FROM dbo.Transactions WHERE CreatedAt >= @Start AND CreatedAt < @End ORDER BY CreatedAt;",
            new { Start = startDate.ToDateTime(TimeOnly.MinValue), End = endDate.AddDays(1).ToDateTime(TimeOnly.MinValue) }, cancellationToken);

    public Task<IEnumerable<Transaction>> GetAllAsync(CancellationToken cancellationToken = default) =>
        QueryAsync("SELECT Id, Type, Amount, Description, CreatedAt FROM dbo.Transactions ORDER BY CreatedAt;", null, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    private async Task<IEnumerable<Transaction>> QueryAsync(string sql, object? parameters, CancellationToken token)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<TransactionRow>(new CommandDefinition(sql, parameters, cancellationToken: token));
        return rows.Select(Map).ToArray();
    }

    private async Task<Transaction?> QuerySingleAsync(string sql, object parameters, CancellationToken token)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<TransactionRow>(new CommandDefinition(sql, parameters, cancellationToken: token));
        return row is null ? null : Map(row);
    }

    private static Transaction Map(TransactionRow row) => Transaction.Rehydrate(row.Id, (TransactionType)row.Type, row.Amount, row.Description, row.CreatedAt);
    private sealed record TransactionRow(Guid Id, int Type, decimal Amount, string Description, DateTime CreatedAt);
}
