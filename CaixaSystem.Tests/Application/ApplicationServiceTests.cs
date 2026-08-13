namespace CaixaSystem.Tests.Application;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CaixaSystem.Application.Services;
using CaixaSystem.Application.Dtos;
using CaixaSystem.Domain.Repositories;
using CaixaSystem.Domain.Entities;
using CaixaSystem.Domain.ValueObjects;
using CaixaSystem.Domain.Exceptions;

/// <summary>
/// Testes para o serviço de transações
/// </summary>
[TestClass]
public class TransactionServiceTests
{
    private Mock<ITransactionRepository> _transactionRepositoryMock = null!;
    private ITransactionService _transactionService = null!;

    [TestInitialize]
    public void Setup()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _transactionService = new TransactionService(_transactionRepositoryMock.Object);
    }

    [TestMethod]
    public async Task CreateCreditAsync_WithValidRequest_ShouldCreateCredit()
    {
        // Arrange
        var request = new CreateCreditTransactionRequest
        {
            Amount = 100,
            Description = "Venda de produtos"
        };

        _transactionRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _transactionRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.CreateCreditAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(100, result.Amount);
        Assert.AreEqual("Credit", result.TypeName);
        Assert.AreEqual("Venda de produtos", result.Description);
        
        _transactionRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _transactionRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task CreateDebitAsync_WithValidRequest_ShouldCreateDebit()
    {
        // Arrange
        var request = new CreateDebitTransactionRequest
        {
            Amount = 50,
            Description = "Pagamento de fornecedor"
        };

        _transactionRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _transactionRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.CreateDebitAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(50, result.Amount);
        Assert.AreEqual("Debit", result.TypeName);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidTransactionException))]
    public async Task CreateCreditAsync_WithZeroAmount_ShouldThrowException()
    {
        // Arrange
        var request = new CreateCreditTransactionRequest
        {
            Amount = 0,
            Description = "Teste"
        };

        // Act
        await _transactionService.CreateCreditAsync(request);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidTransactionException))]
    public async Task CreateCreditAsync_WithNegativeAmount_ShouldThrowException()
    {
        // Arrange
        var request = new CreateCreditTransactionRequest
        {
            Amount = -50,
            Description = "Teste"
        };

        // Act
        await _transactionService.CreateCreditAsync(request);
    }

    [TestMethod]
    public async Task GetByDateAsync_ShouldReturnTransactionsForDate()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 12);
        var transaction1 = Transaction.CreateCredit(Money.Create(100), "Transação 1");
        var transaction2 = Transaction.CreateCredit(Money.Create(50), "Transação 2");

        _transactionRepositoryMock
            .Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { transaction1, transaction2 });

        // Act
        var result = await _transactionService.GetByDateAsync(date);

        // Assert
        Assert.AreEqual(2, result.Count());
        _transactionRepositoryMock.Verify(
            r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

/// <summary>
/// Testes para o serviço de consolidação de saldos diários
/// </summary>
[TestClass]
public class DailyBalanceServiceTests
{
    private Mock<ITransactionRepository> _transactionRepositoryMock = null!;
    private Mock<IDailyBalanceRepository> _dailyBalanceRepositoryMock = null!;
    private IDailyBalanceService _dailyBalanceService = null!;

    [TestInitialize]
    public void Setup()
    {
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _dailyBalanceRepositoryMock = new Mock<IDailyBalanceRepository>();
        _dailyBalanceService = new DailyBalanceService(
            _transactionRepositoryMock.Object,
            _dailyBalanceRepositoryMock.Object);
    }

    [TestMethod]
    public async Task ConsolidateDailyBalanceAsync_WithTransactions_ShouldCalculateBalance()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 12);
        var credit = Transaction.CreateCredit(Money.Create(1000), "Venda");
        var debit = Transaction.CreateDebit(Money.Create(300), "Pagamento");

        _transactionRepositoryMock
            .Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { credit, debit });

        _dailyBalanceRepositoryMock
            .Setup(r => r.GetByDateAsync(date.AddDays(-1), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyBalance?)null);

        _dailyBalanceRepositoryMock
            .Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyBalance?)null);

        _dailyBalanceRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<DailyBalance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dailyBalanceRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _dailyBalanceService.ConsolidateDailyBalanceAsync(date);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1000, result.TotalCredits);
        Assert.AreEqual(300, result.TotalDebits);
        Assert.AreEqual(700, result.DayBalance); // 1000 - 300
        Assert.AreEqual(700, result.ConsolidatedBalance); // 0 + 700
    }

    [TestMethod]
    public async Task ConsolidateDailyBalanceAsync_WithPreviousDayBalance_ShouldAccumulate()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 12);
        var previousDate = date.AddDays(-1);
        var credit = Transaction.CreateCredit(Money.Create(500), "Venda");

        var previousBalance = DailyBalance.Create(
            previousDate,
            Money.Create(1000),
            Money.Create(200),
            Money.Zero);

        _transactionRepositoryMock
            .Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { credit });

        _dailyBalanceRepositoryMock
            .Setup(r => r.GetByDateAsync(previousDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previousBalance);

        _dailyBalanceRepositoryMock
            .Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyBalance?)null);

        _dailyBalanceRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<DailyBalance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _dailyBalanceRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _dailyBalanceService.ConsolidateDailyBalanceAsync(date);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(500, result.TotalCredits);
        Assert.AreEqual(0, result.TotalDebits);
        Assert.AreEqual(500, result.DayBalance);
        Assert.AreEqual(1300, result.ConsolidatedBalance); // 800 + 500
    }

    [TestMethod]
    public async Task GetDailyBalanceAsync_WhenExists_ShouldReturnBalance()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 12);
        var balance = DailyBalance.Create(date, Money.Create(1000), Money.Create(300));

        _dailyBalanceRepositoryMock
            .Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        // Act
        var result = await _dailyBalanceService.GetDailyBalanceAsync(date);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(date.ToString("yyyy-MM-dd"), result.Date);
    }

    [TestMethod]
    public async Task GetDailyBalanceAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        var date = new DateOnly(2026, 8, 12);

        _dailyBalanceRepositoryMock
            .Setup(r => r.GetByDateAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyBalance?)null);

        // Act
        var result = await _dailyBalanceService.GetDailyBalanceAsync(date);

        // Assert
        Assert.IsNull(result);
    }
}
