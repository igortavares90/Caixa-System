namespace CaixaSystem.Infrastructure;

using CaixaSystem.Application.Services;
using CaixaSystem.Domain.Repositories;
using CaixaSystem.Infrastructure.Data;
using CaixaSystem.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
        services.AddSingleton<IDatabaseInitializer, DapperDatabaseInitializer>();
        services.AddScoped<ITransactionRepository, DapperTransactionRepository>();
        services.AddScoped<IDailyBalanceRepository, DapperDailyBalanceRepository>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IDailyBalanceService, DailyBalanceService>();
        return services;
    }

}
