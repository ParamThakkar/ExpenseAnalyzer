using Asp.Versioning;
using Asp.Versioning.Builder;
using ExpenseAnalyzer.Domain;
using ExpenseAnalyzer.Infra.Repos;

namespace ExpenseAnalyzer.Api.Endpoints;

public static class IncomeEndpoints
{
    public static void MapIncomeEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/income")
            .WithTags("Income")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0));

        group.MapGet("/", GetAllAsync)
            .WithName("GetAllIncome")
            .Produces<IEnumerable<Income>>(200);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetIncomeById")
            .Produces<Income>(200)
            .Produces(404);

        group.MapGet("/account/{accountId:guid}", GetByAccountAsync)
            .WithName("GetIncomeByAccount")
            .Produces<IEnumerable<Income>>(200);

        group.MapGet("/category/{categoryId:guid}", GetByCategoryAsync)
            .WithName("GetIncomeByCategory")
            .Produces<IEnumerable<Income>>(200);

        group.MapGet("/daterange", GetByDateRangeAsync)
            .WithName("GetIncomeByDateRange")
            .Produces<IEnumerable<Income>>(200);

        group.MapGet("/account/{accountId:guid}/total", GetTotalByAccountAsync)
            .WithName("GetTotalIncomeByAccount")
            .Produces<decimal>(200);

        group.MapPost("/", CreateAsync)
            .WithName("CreateIncome")
            .Produces<Income>(201)
            .Produces(400);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateIncome")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteIncome")
            .Produces(204)
            .Produces(404);
    }

    static async Task<IResult> GetAllAsync(IIncomeRepository repo, CancellationToken ct)
    {
        var income = await repo.GetAllOrderedByDateAsync(ct);
        return Results.Ok(income);
    }

    static async Task<IResult> GetByIdAsync(Guid id, IIncomeRepository repo, CancellationToken ct)
    {
        var income = await repo.GetByIdAsync(id, ct);
        return income is null ? Results.NotFound() : Results.Ok(income);
    }

    static async Task<IResult> GetByAccountAsync(Guid accountId, IIncomeRepository repo, CancellationToken ct)
    {
        var income = await repo.GetByAccountAsync(accountId, ct);
        return Results.Ok(income);
    }

    static async Task<IResult> GetByCategoryAsync(Guid categoryId, IIncomeRepository repo, CancellationToken ct)
    {
        var income = await repo.GetByCategoryAsync(categoryId, ct);
        return Results.Ok(income);
    }

    static async Task<IResult> GetByDateRangeAsync(DateTime startDate, DateTime endDate, IIncomeRepository repo, CancellationToken ct)
    {
        if (startDate > endDate)
            return Results.BadRequest("Start date must be before end date");

        var income = await repo.GetByDateRangeAsync(startDate, endDate, ct);
        return Results.Ok(income);
    }

    static async Task<IResult> GetTotalByAccountAsync(Guid accountId, IIncomeRepository repo, CancellationToken ct)
    {
        var total = await repo.GetTotalByAccountAsync(accountId, ct);
        return Results.Ok(total);
    }

    static async Task<IResult> CreateAsync(Income income, IIncomeRepository repo, CancellationToken ct)
    {
        if (income.CategoryId == Guid.Empty)
            return Results.BadRequest("CategoryId is required");
        if (income.AccountId == Guid.Empty)
            return Results.BadRequest("AccountId is required");
        if (income.Amount <= 0)
            return Results.BadRequest("Amount must be greater than zero");

        income.Id = Guid.NewGuid();
        await repo.InsertAsync(income, ct);
        await repo.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/income/{income.Id}", income);
    }

    static async Task<IResult> UpdateAsync(Guid id, Income income, IIncomeRepository repo, CancellationToken ct)
    {
        var existing = await repo.GetByIdAsync(id, ct);
        if (existing is null)
            return Results.NotFound();

        if (income.CategoryId == Guid.Empty)
            return Results.BadRequest("CategoryId is required");
        if (income.AccountId == Guid.Empty)
            return Results.BadRequest("AccountId is required");
        if (income.Amount <= 0)
            return Results.BadRequest("Amount must be greater than zero");

        existing.CategoryId = income.CategoryId;
        existing.AccountId = income.AccountId;
        existing.Amount = income.Amount;
        existing.Timestamp = income.Timestamp;
        existing.Comment = income.Comment;

        repo.Update(existing);
        await repo.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    static async Task<IResult> DeleteAsync(Guid id, IIncomeRepository repo, CancellationToken ct)
    {
        var income = await repo.GetByIdAsync(id, ct);
        if (income is null)
            return Results.NotFound();

        repo.Delete(income);
        await repo.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
