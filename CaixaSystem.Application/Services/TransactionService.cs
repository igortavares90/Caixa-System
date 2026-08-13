namespace CaixaSystem.Application.Services;

using CaixaSystem.Domain.Entities;
using CaixaSystem.Domain.Exceptions;
using CaixaSystem.Domain.Repositories;
using CaixaSystem.Domain.ValueObjects;
using CaixaSystem.Application.Dtos;

/// <summary>
/// Serviço para gerenciar transações financeiras.
/// Implementa use cases de criação de transações.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Registra um novo crédito no caixa.
    /// </summary>
    /// <param name="request">Dados da transação de crédito</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados da transação criada</returns>
    Task<TransactionResponse> CreateCreditAsync(
        CreateCreditTransactionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um novo débito no caixa.
    /// </summary>
    /// <param name="request">Dados da transação de débito</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados da transação criada</returns>
    Task<TransactionResponse> CreateDebitAsync(
        CreateDebitTransactionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as transações de um dia.
    /// </summary>
    /// <param name="date">Data das transações</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de transações do dia</returns>
    Task<IEnumerable<TransactionResponse>> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação do serviço de transações.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
    }

    public async Task<TransactionResponse> CreateCreditAsync(
        CreateCreditTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.Amount <= 0)
            throw new InvalidTransactionException("O valor do crédito deve ser maior que zero.");

        var amount = Money.Create(request.Amount);
        var transaction = Transaction.CreateCredit(amount, request.Description);

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _transactionRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(transaction);
    }

    public async Task<TransactionResponse> CreateDebitAsync(
        CreateDebitTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (request.Amount <= 0)
            throw new InvalidTransactionException("O valor do débito deve ser maior que zero.");

        var amount = Money.Create(request.Amount);
        var transaction = Transaction.CreateDebit(amount, request.Description);

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _transactionRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(transaction);
    }

    public async Task<IEnumerable<TransactionResponse>> GetByDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetByDateAsync(date, cancellationToken);
        return transactions.Select(MapToResponse);
    }

    private static TransactionResponse MapToResponse(Transaction transaction)
    {
        return new TransactionResponse
        {
            Id = transaction.Id,
            Type = (int)transaction.Type,
            TypeName = transaction.Type.ToString(),
            Amount = transaction.Amount.Amount,
            Description = transaction.Description,
            CreatedAt = transaction.CreatedAt
        };
    }
}
