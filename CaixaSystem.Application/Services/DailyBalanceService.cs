namespace CaixaSystem.Application.Services;

using CaixaSystem.Domain.Entities;
using CaixaSystem.Domain.Exceptions;
using CaixaSystem.Domain.Repositories;
using CaixaSystem.Domain.ValueObjects;
using CaixaSystem.Application.Dtos;

/// <summary>
/// Serviço para consolidação de saldos diários.
/// Responsável pela agregação e cálculo de saldos consolidados.
/// </summary>
public interface IDailyBalanceService
{
    /// <summary>
    /// Consolida o saldo de um dia específico.
    /// </summary>
    /// <param name="date">Data a ser consolidada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Saldo consolidado do dia</returns>
    Task<DailyBalanceResponse> ConsolidateDailyBalanceAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o saldo consolidado de um dia específico.
    /// </summary>
    /// <param name="date">Data do saldo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Saldo consolidado ou null</returns>
    Task<DailyBalanceResponse?> GetDailyBalanceAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o saldo consolidado mais recente.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Último saldo consolidado</returns>
    Task<DailyBalanceResponse?> GetLatestBalanceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os saldos consolidados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de todos os saldos consolidados</returns>
    Task<IEnumerable<DailyBalanceResponse>> GetAllBalancesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação do serviço de consolidação de saldos diários.
/// </summary>
public class DailyBalanceService : IDailyBalanceService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IDailyBalanceRepository _dailyBalanceRepository;

    public DailyBalanceService(
        ITransactionRepository transactionRepository,
        IDailyBalanceRepository dailyBalanceRepository)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _dailyBalanceRepository = dailyBalanceRepository ?? throw new ArgumentNullException(nameof(dailyBalanceRepository));
    }

    public async Task<DailyBalanceResponse> ConsolidateDailyBalanceAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Busca todas as transações do dia
            var transactions = await _transactionRepository.GetByDateAsync(date, cancellationToken);

            // Calcula totais de crédito e débito
            var totalCredits = transactions
                .Where(t => t.Type == Domain.Enums.TransactionType.Credit)
                .Aggregate(Money.Zero, (acc, t) => acc + t.Amount);

            var totalDebits = transactions
                .Where(t => t.Type == Domain.Enums.TransactionType.Debit)
                .Aggregate(Money.Zero, (acc, t) => acc + t.Amount);

            // Obtém o saldo do dia anterior
            var previousDay = date.AddDays(-1);
            var previousBalance = await _dailyBalanceRepository.GetByDateAsync(previousDay, cancellationToken);
            var previousDayBalance = previousBalance?.ConsolidatedBalance ?? Money.Zero;

            // Cria o saldo consolidado
            var dailyBalance = DailyBalance.Create(date, totalCredits, totalDebits, previousDayBalance);

            // Verifica se já existe saldo para este dia e atualiza ou adiciona
            var existingBalance = await _dailyBalanceRepository.GetByDateAsync(date, cancellationToken);
            if (existingBalance != null)
            {
                await _dailyBalanceRepository.UpdateAsync(dailyBalance, cancellationToken);
            }
            else
            {
                await _dailyBalanceRepository.AddAsync(dailyBalance, cancellationToken);
            }

            await _dailyBalanceRepository.SaveChangesAsync(cancellationToken);

            return MapToResponse(dailyBalance);
        }
        catch (Exception ex) when (!(ex is DomainException))
        {
            throw new DailyBalanceConsolidationException(
                $"Erro ao consolidar saldo para o dia {date:yyyy-MM-dd}", ex);
        }
    }

    public async Task<DailyBalanceResponse?> GetDailyBalanceAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var dailyBalance = await _dailyBalanceRepository.GetByDateAsync(date, cancellationToken);
        return dailyBalance != null ? MapToResponse(dailyBalance) : null;
    }

    public async Task<DailyBalanceResponse?> GetLatestBalanceAsync(CancellationToken cancellationToken = default)
    {
        var dailyBalance = await _dailyBalanceRepository.GetLatestAsync(cancellationToken);
        return dailyBalance != null ? MapToResponse(dailyBalance) : null;
    }

    public async Task<IEnumerable<DailyBalanceResponse>> GetAllBalancesAsync(CancellationToken cancellationToken = default)
    {
        var balances = await _dailyBalanceRepository.GetAllAsync(cancellationToken);
        return balances.Select(MapToResponse);
    }

    private static DailyBalanceResponse MapToResponse(DailyBalance dailyBalance)
    {
        return new DailyBalanceResponse
        {
            Id = dailyBalance.Id,
            Date = dailyBalance.Date.ToString("yyyy-MM-dd"),
            TotalCredits = dailyBalance.TotalCredits.Amount,
            TotalDebits = dailyBalance.TotalDebits.Amount,
            DayBalance = dailyBalance.Balance.Amount,
            ConsolidatedBalance = dailyBalance.ConsolidatedBalance.Amount,
            PreviousDayBalance = dailyBalance.PreviousDayBalance.Amount,
            ConsolidatedAt = dailyBalance.ConsolidatedAt
        };
    }
}
