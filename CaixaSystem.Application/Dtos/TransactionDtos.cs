namespace CaixaSystem.Application.Dtos;

/// <summary>
/// DTO para requisição de criação de transação de crédito.
/// </summary>
public class CreateCreditTransactionRequest
{
    /// <summary>
    /// Valor do crédito
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Descrição da transação
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO para requisição de criação de transação de débito.
/// </summary>
public class CreateDebitTransactionRequest
{
    /// <summary>
    /// Valor do débito
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Descrição da transação
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO para resposta de transação.
/// </summary>
public class TransactionResponse
{
    /// <summary>
    /// ID único da transação
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tipo de transação (1 = Crédito, 2 = Débito)
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Tipo de transação em texto
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Valor da transação
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Descrição da transação
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora da transação
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para requisição de saldo diário.
/// </summary>
public class DailyBalanceRequest
{
    /// <summary>
    /// Data do saldo consolidado (yyyy-MM-dd)
    /// </summary>
    public string Date { get; set; } = string.Empty;
}

/// <summary>
/// DTO para resposta de saldo diário consolidado.
/// </summary>
public class DailyBalanceResponse
{
    /// <summary>
    /// ID único do registro
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Data do saldo (yyyy-MM-dd)
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Total de créditos do dia
    /// </summary>
    public decimal TotalCredits { get; set; }

    /// <summary>
    /// Total de débitos do dia
    /// </summary>
    public decimal TotalDebits { get; set; }

    /// <summary>
    /// Saldo do dia (créditos - débitos)
    /// </summary>
    public decimal DayBalance { get; set; }

    /// <summary>
    /// Saldo consolidado acumulado
    /// </summary>
    public decimal ConsolidatedBalance { get; set; }

    /// <summary>
    /// Saldo anterior
    /// </summary>
    public decimal PreviousDayBalance { get; set; }

    /// <summary>
    /// Data e hora da consolidação
    /// </summary>
    public DateTime ConsolidatedAt { get; set; }
}
