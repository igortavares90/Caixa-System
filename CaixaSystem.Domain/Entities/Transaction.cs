namespace CaixaSystem.Domain.Entities;

using CaixaSystem.Domain.Enums;
using CaixaSystem.Domain.ValueObjects;

/// <summary>
/// Representa uma transação financeira (crédito ou débito) do caixa.
/// </summary>
public class Transaction
{
    /// <summary>
    /// Identificador único da transação.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Tipo de transação (Crédito ou Débito).
    /// </summary>
    public TransactionType Type { get; private set; }

    /// <summary>
    /// Valor monetário da transação.
    /// </summary>
    public Money Amount { get; private set; }

    /// <summary>
    /// Descrição ou referência da transação.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Data e hora em que a transação foi registrada.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Construtor privado para validação.
    /// </summary>
    private Transaction(Guid id, TransactionType type, Money amount, string description, DateTime createdAt)
    {
        Id = id;
        Type = type;
        Amount = amount;
        Description = description;
        CreatedAt = createdAt;
    }

    public static Transaction Rehydrate(Guid id, TransactionType type, decimal amount, string description, DateTime createdAt)
    {
        return new Transaction(id, type, Money.Create(amount), description, createdAt);
    }

    /// <summary>
    /// Factory method para criar uma nova transação de crédito.
    /// </summary>
    /// <param name="amount">Valor do crédito</param>
    /// <param name="description">Descrição da transação</param>
    /// <returns>Nova instância de Transaction</returns>
    public static Transaction CreateCredit(Money amount, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição da transação é obrigatória.", nameof(description));

        return new Transaction(Guid.NewGuid(), TransactionType.Credit, amount, description, DateTime.UtcNow);
    }

    /// <summary>
    /// Factory method para criar uma nova transação de débito.
    /// </summary>
    /// <param name="amount">Valor do débito</param>
    /// <param name="description">Descrição da transação</param>
    /// <returns>Nova instância de Transaction</returns>
    public static Transaction CreateDebit(Money amount, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição da transação é obrigatória.", nameof(description));

        return new Transaction(Guid.NewGuid(), TransactionType.Debit, amount, description, DateTime.UtcNow);
    }

    /// <summary>
    /// Obtém a data da transação sem a hora.
    /// </summary>
    public DateOnly GetTransactionDate()
    {
        return DateOnly.FromDateTime(CreatedAt);
    }
}
