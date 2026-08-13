namespace CaixaSystem.API.Models;

/// <summary>
/// Modelo de resposta genérica da API.
/// </summary>
/// <typeparam name="T">Tipo dos dados da resposta</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indica se a requisição foi bem-sucedida
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensagem da resposta
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Dados da resposta
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Erros da resposta (se houver)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Timestamp da resposta
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operação realizada com sucesso")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

/// <summary>
/// Modelo de resposta para listas
/// </summary>
public class ApiListResponse<T>
{
    /// <summary>
    /// Indica se a requisição foi bem-sucedida
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensagem da resposta
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Lista de dados
    /// </summary>
    public List<T> Data { get; set; } = new();

    /// <summary>
    /// Timestamp da resposta
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiListResponse<T> SuccessResponse(IEnumerable<T> data, string message = "Operação realizada com sucesso")
    {
        return new ApiListResponse<T>
        {
            Success = true,
            Message = message,
            Data = data.ToList()
        };
    }

    public static ApiListResponse<T> ErrorResponse(string message)
    {
        return new ApiListResponse<T>
        {
            Success = false,
            Message = message,
            Data = new()
        };
    }
}
