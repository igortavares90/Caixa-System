namespace CaixaSystem.Domain.Entities;

using CaixaSystem.Domain.ValueObjects;

/// <summary>
/// Representa o saldo consolidado de um dia específico.
/// Armazena o total de créditos, débitos e saldo final do dia.
/// </summary>
public class DailyBalance
{
    /// <summary>
    /// Identificador único do registro de saldo diário.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Data do saldo consolidado (sem hora).
    /// </summary>
    public DateOnly Date { get; private set; }

    /// <summary>
    /// Total de créditos registrados no dia.
    /// </summary>
    public Money TotalCredits { get; private set; }

    /// <summary>
    /// Total de débitos registrados no dia.
    /// </summary>
    public Money TotalDebits { get; private set; }

    /// <summary>
    /// Saldo consolidado do dia (créditos - débitos).
    /// </summary>
    public Money Balance { get; private set; }

    /// <summary>
    /// Saldo do dia anterior, utilizado para calcular o saldo consolidado.
    /// </summary>
    public Money PreviousDayBalance { get; private set; }

    /// <summary>
    /// Saldo consolidado acumulado até este dia.
    /// </summary>
    public Money ConsolidatedBalance { get; private set; }

    /// <summary>
    /// Data e hora em que o saldo foi consolidado.
    /// </summary>
    public DateTime ConsolidatedAt { get; private set; }

    /// <summary>
    /// Construtor privado para validação.
    /// </summary>
    private DailyBalance(
        Guid id,
        DateOnly date,
        Money totalCredits,
        Money totalDebits,
        Money previousDayBalance)
    {
        Id = id;
        Date = date;
        TotalCredits = totalCredits;
        TotalDebits = totalDebits;
        PreviousDayBalance = previousDayBalance;

        // Calcula o saldo do dia
        Balance = Money.Create(totalCredits.Amount - totalDebits.Amount);

        // Calcula o saldo consolidado (dia anterior + saldo do dia)
        ConsolidatedBalance = Money.Create(previousDayBalance.Amount + Balance.Amount);

        ConsolidatedAt = DateTime.UtcNow;
    }

    private DailyBalance(Guid id, DateOnly date, Money totalCredits, Money totalDebits, Money balance,
        Money previousDayBalance, Money consolidatedBalance, DateTime consolidatedAt)
    {
        Id = id;
        Date = date;
        TotalCredits = totalCredits;
        TotalDebits = totalDebits;
        Balance = balance;
        PreviousDayBalance = previousDayBalance;
        ConsolidatedBalance = consolidatedBalance;
        ConsolidatedAt = consolidatedAt;
    }

    public static DailyBalance Rehydrate(Guid id, DateOnly date, decimal totalCredits, decimal totalDebits,
        decimal balance, decimal previousDayBalance, decimal consolidatedBalance, DateTime consolidatedAt)
    {
        return new DailyBalance(id, date, Money.Create(totalCredits), Money.Create(totalDebits),
            Money.Create(balance), Money.Create(previousDayBalance), Money.Create(consolidatedBalance), consolidatedAt);
    }

    /// <summary>
    /// Factory method para criar um novo saldo diário.
    /// </summary>
    /// <param name="date">Data do saldo</param>
    /// <param name="totalCredits">Total de créditos do dia</param>
    /// <param name="totalDebits">Total de débitos do dia</param>
    /// <param name="previousDayBalance">Saldo do dia anterior (padrão: zero)</param>
    /// <returns>Nova instância de DailyBalance</returns>
    public static DailyBalance Create(
        DateOnly date,
        Money totalCredits,
        Money totalDebits,
        Money? previousDayBalance = null)
    {
        previousDayBalance ??= Money.Zero;

        return new DailyBalance(Guid.NewGuid(), date, totalCredits, totalDebits, previousDayBalance);
    }

    /// <summary>
    /// Verifica se o saldo do dia é positivo (superávit).
    /// </summary>
    public bool IsPositive => Balance.IsPositive;

    /// <summary>
    /// Verifica se o saldo consolidado é positivo.
    /// </summary>
    public bool IsConsolidatedBalancePositive => ConsolidatedBalance.IsPositive;

    /// <summary>
    /// Obtém uma representação detalhada do saldo diário.
    /// </summary>
    public override string ToString()
    {
        return $"Data: {Date:yyyy-MM-dd}, Créditos: {TotalCredits}, Débitos: {TotalDebits}, Saldo do Dia: {Balance}, Saldo Consolidado: {ConsolidatedBalance}";
    }
}
