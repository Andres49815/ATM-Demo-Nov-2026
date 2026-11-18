using ATM.Application.Persistence;
using ATM.Contracts;

namespace ATM.Application.Queries;

public sealed class DashboardQueryHandler(IAtmRepository repository, AtmOperationGate gate)
{
    public Task<DashboardResponse> HandleAsync(CancellationToken cancellationToken) =>
        gate.RunAsync(async () =>
        {
            var accounts = await repository.ListAccountsAsync(cancellationToken);
            var transactions = await repository.ListTransactionsAsync(cancellationToken);
            return new DashboardResponse(
                accounts.Select(a => new AccountResponse(a.Id, a.Name, a.Balance)).ToArray(),
                transactions.Select(t => new TransactionResponse(
                    t.Id, t.Kind.ToString(), t.Amount, t.FromAccountId, t.ToAccountId, t.OccurredAt)).ToArray());
        }, cancellationToken);
}
