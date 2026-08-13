namespace CaixaSystem.Domain.Exceptions;

/// <summary>
/// Exceção base para erros de domínio.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Inicializa uma nova instância de DomainException.
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    protected DomainException(string message) : base(message)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância de DomainException com exceção interna.
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    /// <param name="innerException">Exceção interna</param>
    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exceção lançada quando há erro na validação de transação.
/// </summary>
public class InvalidTransactionException : DomainException
{
    /// <summary>
    /// Inicializa uma nova instância de InvalidTransactionException.
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    public InvalidTransactionException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exceção lançada quando há erro na consolidação do saldo diário.
/// </summary>
public class DailyBalanceConsolidationException : DomainException
{
    /// <summary>
    /// Inicializa uma nova instância de DailyBalanceConsolidationException.
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    public DailyBalanceConsolidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância de DailyBalanceConsolidationException com exceção interna.
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    /// <param name="innerException">Exceção interna</param>
    public DailyBalanceConsolidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exceção lançada quando um recurso não é encontrado.
/// </summary>
public class NotFoundException : DomainException
{
    /// <summary>
    /// Inicializa uma nova instância de NotFoundException.
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    public NotFoundException(string message) : base(message)
    {
    }
}
