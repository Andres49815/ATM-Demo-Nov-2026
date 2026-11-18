using ATM.Application.Commands;
using ATM.Application.Queries;
using ATM.Contracts;
using ATM.Domain;

namespace ATM.Api;

internal static class AtmEndpoints
{
    public static void MapAtmEndpoints(this WebApplication app)
    {
        var atm = app.MapGroup("/api/atm");
        atm.MapGet("/dashboard", async (DashboardQueryHandler query, CancellationToken ct) =>
            Results.Ok(await query.HandleAsync(ct)));

        atm.MapPost("/deposits", (AccountAmountRequest request, AtmCommandHandler handler, CancellationToken ct) =>
            ExecuteAsync(() => handler.HandleAsync(new DepositCommand(request.AccountId, request.Amount), ct)));

        atm.MapPost("/withdrawals", (AccountAmountRequest request, AtmCommandHandler handler, CancellationToken ct) =>
            ExecuteAsync(() => handler.HandleAsync(new WithdrawCommand(request.AccountId, request.Amount), ct)));

        atm.MapPost("/transfers", (TransferRequest request, AtmCommandHandler handler, CancellationToken ct) =>
            ExecuteAsync(() => handler.HandleAsync(new TransferCommand(request.FromAccountId, request.ToAccountId, request.Amount), ct)));
    }

    private static async Task<IResult> ExecuteAsync(Func<Task> handle)
    {
        try
        {
            await handle();
            return Results.NoContent();
        }
        catch (AccountNotFoundException exception)
        {
            return Problem(404, "account_not_found", exception.Message);
        }
        catch (AtmRuleException exception)
        {
            var statusCode = exception.Code is "insufficient_funds" or "balance_limit" ? 409 : 400;
            return Problem(statusCode, exception.Code, exception.Message);
        }
    }

    private static IResult Problem(int status, string code, string detail) =>
        Results.Problem(statusCode: status, detail: detail, extensions: new Dictionary<string, object?> { ["code"] = code });
}
