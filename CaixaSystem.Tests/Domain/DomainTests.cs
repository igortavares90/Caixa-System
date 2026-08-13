namespace CaixaSystem.Tests.Domain;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CaixaSystem.Domain.ValueObjects;
using CaixaSystem.Domain.Entities;
using CaixaSystem.Domain.Enums;

/// <summary>
/// Testes para o Value Object Money
/// </summary>
[TestClass]
public class MoneyTests
{
    [TestMethod]
    public void Create_WithPositiveAmount_ShouldCreateMoney()
    {
        // Arrange & Act
        var money = Money.Create(100.50m);

        // Assert
        Assert.AreEqual(100.50m, money.Amount);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Create_WithNegativeAmount_ShouldThrowException()
    {
        // Act
        Money.Create(-10);
    }

    [TestMethod]
    public void Create_WithZero_ShouldCreateMoney()
    {
        // Arrange & Act
        var money = Money.Create(0);

        // Assert
        Assert.AreEqual(0, money.Amount);
        Assert.IsTrue(money.IsZero);
    }

    [TestMethod]
    public void Add_TwoMoneyValues_ShouldReturnSum()
    {
        // Arrange
        var money1 = Money.Create(100);
        var money2 = Money.Create(50);

        // Act
        var result = money1 + money2;

        // Assert
        Assert.AreEqual(150, result.Amount);
    }

    [TestMethod]
    public void Subtract_TwoMoneyValues_ShouldReturnDifference()
    {
        // Arrange
        var money1 = Money.Create(100);
        var money2 = Money.Create(30);

        // Act
        var result = money1 - money2;

        // Assert
        Assert.AreEqual(70, result.Amount);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Subtract_SubtractingMoreThanAvailable_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(50);
        var money2 = Money.Create(100);

        // Act
        var result = money1 - money2;
    }

    [TestMethod]
    public void Equals_TwoMoneyWithSameAmount_ShouldBeEqual()
    {
        // Arrange
        var money1 = Money.Create(100);
        var money2 = Money.Create(100);

        // Act & Assert
        Assert.AreEqual(money1, money2);
        Assert.IsTrue(money1.Equals(money2));
    }

    [TestMethod]
    public void IsPositive_PositiveMoney_ShouldBeTrue()
    {
        // Arrange
        var money = Money.Create(100);

        // Act & Assert
        Assert.IsTrue(money.IsPositive);
    }

    [TestMethod]
    public void IsPositive_ZeroMoney_ShouldBeFalse()
    {
        // Arrange
        var money = Money.Zero;

        // Act & Assert
        Assert.IsFalse(money.IsPositive);
    }
}

/// <summary>
/// Testes para a entidade Transaction
/// </summary>
[TestClass]
public class TransactionTests
{
    [TestMethod]
    public void CreateCredit_WithValidData_ShouldCreateTransaction()
    {
        // Arrange
        var amount = Money.Create(100);

        // Act
        var transaction = Transaction.CreateCredit(amount, "Venda de produtos");

        // Assert
        Assert.AreEqual(TransactionType.Credit, transaction.Type);
        Assert.AreEqual(100, transaction.Amount.Amount);
        Assert.AreEqual("Venda de produtos", transaction.Description);
        Assert.IsNotNull(transaction.Id);
    }

    [TestMethod]
    public void CreateDebit_WithValidData_ShouldCreateTransaction()
    {
        // Arrange
        var amount = Money.Create(50);

        // Act
        var transaction = Transaction.CreateDebit(amount, "Pagamento de fornecedor");

        // Assert
        Assert.AreEqual(TransactionType.Debit, transaction.Type);
        Assert.AreEqual(50, transaction.Amount.Amount);
        Assert.AreEqual("Pagamento de fornecedor", transaction.Description);
        Assert.IsNotNull(transaction.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateCredit_WithEmptyDescription_ShouldThrowException()
    {
        // Arrange
        var amount = Money.Create(100);

        // Act
        Transaction.CreateCredit(amount, "");
    }

    [TestMethod]
    public void GetTransactionDate_ShouldReturnDateOnly()
    {
        // Arrange
        var amount = Money.Create(100);
        var transaction = Transaction.CreateCredit(amount, "Teste");
        var expectedDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var result = transaction.GetTransactionDate();

        // Assert
        Assert.AreEqual(expectedDate, result);
    }
}

/// <summary>
/// Testes para a entidade DailyBalance
/// </summary>
[TestClass]
public class DailyBalanceTests
{
    [TestMethod]
    public void Create_WithValidData_ShouldCreateDailyBalance()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 12);
        var credits = Money.Create(1000);
        var debits = Money.Create(300);
        var previousBalance = Money.Create(500);

        // Act
        var balance = DailyBalance.Create(date, credits, debits, previousBalance);

        // Assert
        Assert.AreEqual(date, balance.Date);
        Assert.AreEqual(1000, balance.TotalCredits.Amount);
        Assert.AreEqual(300, balance.TotalDebits.Amount);
        Assert.AreEqual(700, balance.Balance.Amount); // 1000 - 300
        Assert.AreEqual(1200, balance.ConsolidatedBalance.Amount); // 500 + 700
    }

    [TestMethod]
    public void Create_WithoutPreviousBalance_ShouldDefaultToZero()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 12);
        var credits = Money.Create(500);
        var debits = Money.Create(100);

        // Act
        var balance = DailyBalance.Create(date, credits, debits);

        // Assert
        Assert.AreEqual(0, balance.PreviousDayBalance.Amount);
        Assert.AreEqual(400, balance.Balance.Amount);
        Assert.AreEqual(400, balance.ConsolidatedBalance.Amount);
    }

    [TestMethod]
    public void IsPositive_WithPositiveDayBalance_ShouldBeTrue()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 12);
        var credits = Money.Create(1000);
        var debits = Money.Create(300);
        var balance = DailyBalance.Create(date, credits, debits);

        // Act & Assert
        Assert.IsTrue(balance.IsPositive);
    }

    [TestMethod]
    public void IsConsolidatedBalancePositive_WithPositiveConsolidated_ShouldBeTrue()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 12);
        var credits = Money.Create(500);
        var debits = Money.Create(100);
        var previousBalance = Money.Create(200);
        var balance = DailyBalance.Create(date, credits, debits, previousBalance);

        // Act & Assert
        Assert.IsTrue(balance.IsConsolidatedBalancePositive);
    }
}
