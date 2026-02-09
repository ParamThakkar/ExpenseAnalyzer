using ExpenseAnalyzer.Domain;
using ExpenseAnalyzer.Infra;
using ExpenseAnalyzer.Infra.Repos;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class InfraRegistrar
{
    public static void AddInfra(this IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddDbContext<ExpenseContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });
            options.UseLazyLoadingProxies();
        });

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IIncomeRepository, IncomeRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
    }
}