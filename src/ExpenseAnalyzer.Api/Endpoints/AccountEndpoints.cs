using Microsoft.Extensions.DependencyInjection;

using Asp.Versioning.Builder;
using Asp.Versioning;

using ExpenseAnalyzer.Infra.Repos;
public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/accounts")
                        .WithTags("Account")
                        .WithApiVersionSet(versionSet)
                        .HasApiVersion(new ApiVersion(1, 0));

        group.MapGet("/", GetAllTasksAsync);

    }

    public static Task GetAllTasksAsync(IAccountRepository accountRepository, CancellationToken ct = default)
    {
        return accountRepository.GetAllAsync(ct);
    }
}