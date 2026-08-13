namespace CaixaSystem.Domain.Enums;

/// <summary>
/// Define os tipos de transações financeiras suportadas pelo sistema.
/// </summary>
public enum TransactionType
{
    /// <summary>Crédito - Entrada de dinheiro</summary>
    Credit = 1,

    /// <summary>Débito - Saída de dinheiro</summary>
    Debit = 2
}
