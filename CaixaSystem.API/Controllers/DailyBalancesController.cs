namespace CaixaSystem.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using CaixaSystem.Application.Dtos;
using CaixaSystem.Application.Services;
using CaixaSystem.API.Models;

/// <summary>
/// Controller para gerenciar saldos diários consolidados
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DailyBalancesController : ControllerBase
{
    private readonly IDailyBalanceService _dailyBalanceService;
    private readonly ILogger<DailyBalancesController> _logger;

    public DailyBalancesController(
        IDailyBalanceService dailyBalanceService,
        ILogger<DailyBalancesController> logger)
    {
        _dailyBalanceService = dailyBalanceService ?? throw new ArgumentNullException(nameof(dailyBalanceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Consolida o saldo de um dia específico
    /// </summary>
    /// <param name="date">Data a ser consolidada (yyyy-MM-dd)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Saldo consolidado do dia</returns>
    [HttpPost("consolidate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DailyBalanceResponse>>> ConsolidateBalance(
        [FromQuery] string date,
        CancellationToken cancellationToken = default)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(ApiResponse<DailyBalanceResponse>.ErrorResponse("Formato de data inválido. Use yyyy-MM-dd"));

        _logger.LogInformation("Consolidando saldo para a data: {Date}", date);

        var balance = await _dailyBalanceService.ConsolidateDailyBalanceAsync(parsedDate, cancellationToken);

        return Ok(ApiResponse<DailyBalanceResponse>.SuccessResponse(
            balance,
            $"Saldo consolidado com sucesso para {date}"));
    }

    /// <summary>
    /// Obtém o saldo consolidado de um dia específico
    /// </summary>
    /// <param name="date">Data do saldo (yyyy-MM-dd)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Saldo consolidado ou 404 se não existir</returns>
    [HttpGet("by-date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DailyBalanceResponse>>> GetByDate(
        [FromQuery] string date,
        CancellationToken cancellationToken = default)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest(ApiResponse<DailyBalanceResponse>.ErrorResponse("Formato de data inválido. Use yyyy-MM-dd"));

        _logger.LogInformation("Buscando saldo consolidado para a data: {Date}", date);

        var balance = await _dailyBalanceService.GetDailyBalanceAsync(parsedDate, cancellationToken);

        if (balance is null)
            return NotFound(ApiResponse<DailyBalanceResponse>.ErrorResponse($"Nenhum saldo consolidado encontrado para {date}"));

        return Ok(ApiResponse<DailyBalanceResponse>.SuccessResponse(balance));
    }

    /// <summary>
    /// Obtém o saldo consolidado mais recente
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Último saldo consolidado ou 404 se nenhum existir</returns>
    [HttpGet("latest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DailyBalanceResponse>>> GetLatest(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Buscando último saldo consolidado");

        var balance = await _dailyBalanceService.GetLatestBalanceAsync(cancellationToken);

        if (balance is null)
            return NotFound(ApiResponse<DailyBalanceResponse>.ErrorResponse("Nenhum saldo consolidado disponível"));

        return Ok(ApiResponse<DailyBalanceResponse>.SuccessResponse(balance));
    }

    /// <summary>
    /// Obtém todos os saldos consolidados
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de saldos consolidados</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiListResponse<DailyBalanceResponse>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Buscando todos os saldos consolidados");

        var balances = await _dailyBalanceService.GetAllBalancesAsync(cancellationToken);

        return Ok(ApiListResponse<DailyBalanceResponse>.SuccessResponse(
            balances,
            "Saldos consolidados recuperados com sucesso"));
    }
}
