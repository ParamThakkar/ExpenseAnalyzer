using Asp.Versioning;
using Asp.Versioning.Builder;
using ExpenseAnalyzer.Domain;
using ExpenseAnalyzer.Infra.Repos;

namespace ExpenseAnalyzer.Api.Endpoints;

/// <summary>
/// Endpoints for Transfer entity operations.
/// </summary>
public static class TransferEndpoints
{
    public static void MapTransferEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/transfer")
            .WithTags("Transfer")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0));

        group.MapGet("/", GetAllAsync)
            .WithName("GetAllTransfers")
            .Produces<IEnumerable<Transfer>>(200);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetTransferById")
            .Produces<Transfer>(200)
            .Produces(404);

        group.MapGet("/account/{accountId:guid}", GetByAccountAsync)
            .WithName("GetTransfersByAccount")
            .Produces<IEnumerable<Transfer>>(200);

        group.MapGet("/outgoing/{accountId:guid}", GetByOutgoingAccountAsync)
            .WithName("GetTransfersByOutgoingAccount")
            .Produces<IEnumerable<Transfer>>(200);

        group.MapGet("/incoming/{accountId:guid}", GetByIncomingAccountAsync)
            .WithName("GetTransfersByIncomingAccount")
            .Produces<IEnumerable<Transfer>>(200);

        group.MapGet("/daterange", GetByDateRangeAsync)
            .WithName("GetTransfersByDateRange")
            .Produces<IEnumerable<Transfer>>(200);

        group.MapPost("/", CreateAsync)
            .WithName("CreateTransfer")
            .Produces<Transfer>(201)
            .Produces(400);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateTransfer")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteTransfer")
            .Produces(204)
            .Produces(404);
    }

    public static async Task<IResult> GetAllAsync(ITransferRepository repo, CancellationToken ct)
    {
        var transfers = await repo.GetAllOrderedByDateAsync(ct);
        return Results.Ok(transfers);
    }

    public static async Task<IResult> GetByIdAsync(Guid id, ITransferRepository repo, CancellationToken ct)
    {
        var transfer = await repo.GetByIdAsync(id, ct);
        return transfer is null ? Results.NotFound() : Results.Ok(transfer);
    }

    public static async Task<IResult> GetByAccountAsync(Guid accountId, ITransferRepository repo, CancellationToken ct)
    {
        var transfers = await repo.GetByAccountAsync(accountId, ct);
        return Results.Ok(transfers);
    }

    public static async Task<IResult> GetByOutgoingAccountAsync(Guid accountId, ITransferRepository repo, CancellationToken ct)
    {
        var transfers = await repo.GetByOutgoingAccountAsync(accountId, ct);
        return Results.Ok(transfers);
    }

    public static async Task<IResult> GetByIncomingAccountAsync(Guid accountId, ITransferRepository repo, CancellationToken ct)
    {
        var transfers = await repo.GetByIncomingAccountAsync(accountId, ct);
        return Results.Ok(transfers);
    }

    public static async Task<IResult> GetByDateRangeAsync(DateTime startDate, DateTime endDate, ITransferRepository repo, CancellationToken ct)
    {
        if (startDate > endDate)
            return Results.BadRequest("Start date must be before end date");

        var transfers = await repo.GetByDateRangeAsync(startDate, endDate, ct);
        return Results.Ok(transfers);
    }

    public static async Task<IResult> CreateAsync(Transfer transfer, ITransferRepository repo, CancellationToken ct)
    {
        if (transfer.OutgoingAccountId == Guid.Empty)
            return Results.BadRequest("OutgoingAccountId is required");
        if (transfer.IncomingAccountId == Guid.Empty)
            return Results.BadRequest("IncomingAccountId is required");
        if (transfer.OutgoingAccountId == transfer.IncomingAccountId)
            return Results.BadRequest("OutgoingAccountId and IncomingAccountId must be different");
        if (transfer.Amount <= 0)
            return Results.BadRequest("Amount must be greater than zero");

        transfer.Id = Guid.NewGuid();
        await repo.InsertAsync(transfer, ct);
        await repo.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/transfer/{transfer.Id}", transfer);
    }

    public static async Task<IResult> UpdateAsync(Guid id, Transfer transfer, ITransferRepository repo, CancellationToken ct)
    {
        var existing = await repo.GetByIdAsync(id, ct);
        if (existing is null)
            return Results.NotFound();

        if (transfer.OutgoingAccountId == Guid.Empty)
            return Results.BadRequest("OutgoingAccountId is required");
        if (transfer.IncomingAccountId == Guid.Empty)
            return Results.BadRequest("IncomingAccountId is required");
        if (transfer.OutgoingAccountId == transfer.IncomingAccountId)
            return Results.BadRequest("OutgoingAccountId and IncomingAccountId must be different");
        if (transfer.Amount <= 0)
            return Results.BadRequest("Amount must be greater than zero");

        existing.OutgoingAccountId = transfer.OutgoingAccountId;
        existing.IncomingAccountId = transfer.IncomingAccountId;
        existing.Amount = transfer.Amount;
        existing.Timestamp = transfer.Timestamp;
        existing.Comment = transfer.Comment;

        repo.Update(existing);
        await repo.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    public static async Task<IResult> DeleteAsync(Guid id, ITransferRepository repo, CancellationToken ct)
    {
        var transfer = await repo.GetByIdAsync(id, ct);
        if (transfer is null)
            return Results.NotFound();

        repo.Delete(transfer);
        await repo.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
