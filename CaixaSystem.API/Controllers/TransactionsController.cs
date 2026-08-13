namespace CaixaSystem.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CaixaSystem.Application.Dtos;
using CaixaSystem.Application.Services;
using CaixaSystem.API.Models;

/// <summary>
/// Controller para gerenciar transações financeiras
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        ITransactionService transactionService,
        ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registra um novo crédito no caixa
    /// </summary>
    /// <param name="request">Dados da transação de crédito</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Transação criada</returns>
    [HttpPost("credit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> CreateCredit(
        [FromBody] CreateCreditTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Criando novo crédito: {Amount}", request.Amount);

        var transaction = await _transactionService.CreateCreditAsync(request, cancellationToken);

        return Ok(ApiResponse<TransactionResponse>.SuccessResponse(
            transaction,
            "Crédito registrado com sucesso"));
    }

    /// <summary>
    /// Registra um novo débito no caixa
    /// </summary>
    /// <param name="request">Dados da transação de débito</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Transação criada</returns>
    [HttpPost("debit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> CreateDebit(
        [FromBody] CreateDebitTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Criando novo débito: {Amount}", request.Amount);

        var transaction = await _transactionService.CreateDebitAsync(request, cancellationToken);

        return Ok(ApiResponse<TransactionResponse>.SuccessResponse(
            transaction,
            "Débito registrado com sucesso"));
    }

    /// <summary>
    /// Obtém todas as transações de um dia específico
    /// </summary>
    /// <param name="date">Data das transações (yyyy-MM-dd)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de transações do dia</returns>
    [HttpGet("by-date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiListResponse<TransactionResponse>>> GetByDate(
        [FromQuery] string date,
        CancellationToken cancellationToken = default)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(ApiListResponse<TransactionResponse>.ErrorResponse("Formato de data inválido. Use yyyy-MM-dd"));

        _logger.LogInformation("Buscando transações para a data: {Date}", date);

        var transactions = await _transactionService.GetByDateAsync(parsedDate, cancellationToken);

        return Ok(ApiListResponse<TransactionResponse>.SuccessResponse(
            transactions,
            $"Transações encontradas para {date}"));
    }
}
