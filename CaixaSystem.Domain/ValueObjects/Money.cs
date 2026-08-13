namespace CaixaSystem.Domain.ValueObjects;

/// <summary>
/// Representa um valor monetário com precisão decimal.
/// Implementa o padrão Value Object imutável.
/// </summary>
public class Money : IEquatable<Money>
{
    /// <summary>
    /// Valor monetário em unidades da moeda (ex: reais).
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Construtor privado para evitar criação direta de instâncias com valores inválidos.
    /// </summary>
    private Money(decimal amount)
    {
        Amount = amount;
    }

    /// <summary>
    /// Cria uma nova instância de Money após validação.
    /// </summary>
    /// <param name="amount">Valor monetário</param>
    /// <returns>Instância de Money</returns>
    /// <exception cref="ArgumentException">Quando o valor é negativo</exception>
    public static Money Create(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("O valor monetário não pode ser negativo.", nameof(amount));

        return new Money(amount);
    }

    /// <summary>
    /// Cria uma instância de Money com valor zero.
    /// </summary>
    public static Money Zero => new(0);

    /// <summary>
    /// Adiciona dois valores monetários.
    /// </summary>
    public static Money operator +(Money left, Money right)
    {
        return new Money(left.Amount + right.Amount);
    }

    /// <summary>
    /// Subtrai dois valores monetários.
    /// </summary>
    public static Money operator -(Money left, Money right)
    {
        if (left.Amount < right.Amount)
            throw new InvalidOperationException("Não é possível subtrair um valor maior do que o saldo atual.");

        return new Money(left.Amount - right.Amount);
    }

    /// <summary>
    /// Compara igualdade entre dois valores monetários.
    /// </summary>
    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount;
    }

    /// <summary>
    /// Compara igualdade com outro objeto.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is Money money)
            return Equals(money);
        return false;
    }

    /// <summary>
    /// Retorna o hash code do objeto.
    /// </summary>
    public override int GetHashCode()
    {
        return Amount.GetHashCode();
    }

    /// <summary>
    /// Representa o valor monetário em formato de string.
    /// </summary>
    public override string ToString()
    {
        return $"R$ {Amount:F2}";
    }

    /// <summary>
    /// Verifica se o valor monetário é zero.
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// Verifica se o valor monetário é positivo.
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Verifica se o valor monetário é maior que outro.
    /// </summary>
    public bool IsGreaterThan(Money other)
    {
        return Amount > other.Amount;
    }

    /// <summary>
    /// Verifica se o valor monetário é menor que outro.
    /// </summary>
    public bool IsLessThan(Money other)
    {
        return Amount < other.Amount;
    }
}
