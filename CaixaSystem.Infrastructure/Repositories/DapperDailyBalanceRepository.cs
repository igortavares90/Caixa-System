namespace CaixaSystem.Infrastructure.Repositories;

using CaixaSystem.Domain.Entities;
using CaixaSystem.Domain.Repositories;
using CaixaSystem.Infrastructure.Data;
using Dapper;

public sealed class DapperDailyBalanceRepository : IDailyBalanceRepository
{
    private const string Columns = "Id, Date, TotalCredits, TotalDebits, Balance, PreviousDayBalance, ConsolidatedBalance, ConsolidatedAt";
    private readonly IDbConnectionFactory _connectionFactory;
    public DapperDailyBalanceRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task AddAsync(DailyBalance value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        const string sql = "INSERT INTO dbo.DailyBalances (Id, Date, TotalCredits, TotalDebits, Balance, PreviousDayBalance, ConsolidatedBalance, ConsolidatedAt) VALUES (@Id, @Date, @TotalCredits, @TotalDebits, @Balance, @PreviousDayBalance, @ConsolidatedBalance, @ConsolidatedAt);";
        await ExecuteAsync(sql, Parameters(value), cancellationToken);
    }

    public Task<DailyBalance?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default) =>
        QuerySingleAsync($"SELECT {Columns} FROM dbo.DailyBalances WHERE Date = @Date;", new { Date = date.ToDateTime(TimeOnly.MinValue) }, cancellationToken);

    public Task<DailyBalance?> GetLatestAsync(CancellationToken cancellationToken = default) =>
        QuerySingleAsync($"SELECT TOP (1) {Columns} FROM dbo.DailyBalances ORDER BY Date DESC;", null, cancellationToken);

    public Task<IEnumerable<DailyBalance>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default) =>
        QueryAsync($"SELECT {Columns} FROM dbo.DailyBalances WHERE Date >= @Start AND Date <= @End ORDER BY Date;",
            new { Start = startDate.ToDateTime(TimeOnly.MinValue), End = endDate.ToDateTime(TimeOnly.MinValue) }, cancellationToken);

    public Task<IEnumerable<DailyBalance>> GetAllAsync(CancellationToken cancellationToken = default) =>
        QueryAsync($"SELECT {Columns} FROM dbo.DailyBalances ORDER BY Date;", null, cancellationToken);

    public async Task UpdateAsync(DailyBalance value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        const string sql = "UPDATE dbo.DailyBalances SET Id=@Id, TotalCredits=@TotalCredits, TotalDebits=@TotalDebits, Balance=@Balance, PreviousDayBalance=@PreviousDayBalance, ConsolidatedBalance=@ConsolidatedBalance, ConsolidatedAt=@ConsolidatedAt WHERE Date=@Date;";
        var affected = await ExecuteAsync(sql, Parameters(value), cancellationToken);
        if (affected == 0) throw new InvalidOperationException($"DailyBalance for date {value.Date:yyyy-MM-dd} not found.");
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    private async Task<int> ExecuteAsync(string sql, object parameters, CancellationToken token)
    {
        await using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: token));
    }

    private async Task<IEnumerable<DailyBalance>> QueryAsync(string sql, object? parameters, CancellationToken token)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<DailyBalanceRow>(new CommandDefinition(sql, parameters, cancellationToken: token));
        return rows.Select(Map).ToArray();
    }

    private async Task<DailyBalance?> QuerySingleAsync(string sql, object? parameters, CancellationToken token)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<DailyBalanceRow>(new CommandDefinition(sql, parameters, cancellationToken: token));
        return row is null ? null : Map(row);
    }

    private static object Parameters(DailyBalance value) => new
    {
        value.Id,
        Date = value.Date.ToDateTime(TimeOnly.MinValue),
        TotalCredits = value.TotalCredits.Amount,
        TotalDebits = value.TotalDebits.Amount,
        Balance = value.Balance.Amount,
        PreviousDayBalance = value.PreviousDayBalance.Amount,
        ConsolidatedBalance = value.ConsolidatedBalance.Amount,
        value.ConsolidatedAt
    };

    private static DailyBalance Map(DailyBalanceRow row) => DailyBalance.Rehydrate(row.Id, DateOnly.FromDateTime(row.Date), row.TotalCredits,
        row.TotalDebits, row.Balance, row.PreviousDayBalance, row.ConsolidatedBalance, row.ConsolidatedAt);

    private sealed record DailyBalanceRow(Guid Id, DateTime Date, decimal TotalCredits, decimal TotalDebits, decimal Balance,
        decimal PreviousDayBalance, decimal ConsolidatedBalance, DateTime ConsolidatedAt);
}
