namespace CaixaSystem.Domain.Repositories;

using CaixaSystem.Domain.Entities;

/// <summary>
/// Interface para persistência de transações.
/// Define operações CRUD para transações financeiras.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Adiciona uma nova transação ao repositório.
    /// </summary>
    /// <param name="transaction">Transação a ser adicionada</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task assíncrona</returns>
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as transações de um dia específico.
    /// </summary>
    /// <param name="date">Data das transações</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Coleção de transações do dia</returns>
    Task<IEnumerable<Transaction>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as transações registradas.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Todas as transações</returns>
    Task<IEnumerable<Transaction>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém transações entre um intervalo de datas.
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Transações no intervalo</returns>
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma transação por ID.
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Transação encontrada ou null</returns>
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Salva as alterações (commit) no repositório.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task assíncrona</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface para persistência de saldos diários consolidados.
/// Define operações para armazenar e recuperar saldos consolidados.
/// </summary>
public interface IDailyBalanceRepository
{
    /// <summary>
    /// Adiciona um novo saldo diário consolidado.
    /// </summary>
    /// <param name="dailyBalance">Saldo diário a ser adicionado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task assíncrona</returns>
    Task AddAsync(DailyBalance dailyBalance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o saldo consolidado de um dia específico.
    /// </summary>
    /// <param name="date">Data do saldo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Saldo consolidado ou null se não existir</returns>
    Task<DailyBalance?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os saldos consolidados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Todos os saldos consolidados</returns>
    Task<IEnumerable<DailyBalance>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o saldo consolidado mais recente.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Último saldo consolidado ou null</returns>
    Task<DailyBalance?> GetLatestAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém saldos consolidados entre um intervalo de datas.
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Saldos no intervalo</returns>
    Task<IEnumerable<DailyBalance>> GetByDateRangeAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um saldo consolidado existente.
    /// </summary>
    /// <param name="dailyBalance">Saldo consolidado atualizado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task assíncrona</returns>
    Task UpdateAsync(DailyBalance dailyBalance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Salva as alterações (commit) no repositório.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task assíncrona</returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
