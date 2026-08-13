using CaixaSystem.Infrastructure;
using CaixaSystem.Infrastructure.Data;
using CaixaSystem.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

// Registrar a infraestrutura Dapper com SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A connection string 'DefaultConnection' não foi configurada.");
builder.Services.AddInfrastructure(connectionString);

// Configurar logging
builder.Services.AddLogging();

var app = builder.Build();

// Garantir que o banco e suas tabelas existam
try
{
    using (var scope = app.Services.CreateScope())
    {
        var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.InitializeAsync();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Não foi possível inicializar o banco de dados.");
    throw;
}

// Middleware de tratamento de exceções
app.UseExceptionHandling();

// Configure o pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger"))
        .ExcludeFromDescription();
}

app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    status = "Healthy", 
    timestamp = DateTime.UtcNow,
    database = "SQLServer"
}))
    .WithName("Health Check")
    .WithOpenApi();

app.Run();
