using ATM.Application.Persistence;
using ATM.Domain;

namespace ATM.Application.Commands;

public sealed record DepositCommand(string AccountId, decimal Amount);
public sealed record WithdrawCommand(string AccountId, decimal Amount);
public sealed record TransferCommand(string FromAccountId, string ToAccountId, decimal Amount);

public sealed class AtmCommandHandler(IAtmRepository repository, AtmOperationGate gate, TimeProvider clock)
{
    public Task<long> HandleAsync(DepositCommand command, CancellationToken cancellationToken) =>
        gate.RunAsync(async () =>
        {
            var account = await GetAccountAsync(command.AccountId, cancellationToken);
            account.Deposit(command.Amount);
            return await RecordAsync(TransactionKind.Deposit, command.Amount, null, account.Id, cancellationToken);
        }, cancellationToken);

    public Task<long> HandleAsync(WithdrawCommand command, CancellationToken cancellationToken) =>
        gate.RunAsync(async () =>
        {
            var account = await GetAccountAsync(command.AccountId, cancellationToken);
            account.Withdraw(command.Amount);
            return await RecordAsync(TransactionKind.Withdrawal, command.Amount, account.Id, null, cancellationToken);
        }, cancellationToken);

    public Task<long> HandleAsync(TransferCommand command, CancellationToken cancellationToken) =>
        gate.RunAsync(async () =>
        {
            var source = await GetAccountAsync(command.FromAccountId, cancellationToken);
            var destination = await GetAccountAsync(command.ToAccountId, cancellationToken);
            source.TransferTo(destination, command.Amount);
            return await RecordAsync(TransactionKind.Transfer, command.Amount, source.Id, destination.Id, cancellationToken);
        }, cancellationToken);

    private async Task<Account> GetAccountAsync(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new AtmRuleException("account_required", "Choose an account for the transaction.");

        return await repository.FindAccountAsync(id, cancellationToken)
            ?? throw new AccountNotFoundException(id);
    }

    private async Task<long> RecordAsync(
        TransactionKind kind, decimal amount, string? from, string? to, CancellationToken cancellationToken)
    {
        var transaction = new Transaction(kind, amount, from, to, clock.GetUtcNow());
        await repository.SaveTransactionAsync(transaction, cancellationToken);
        return transaction.Id;
    }
}

public sealed class AccountNotFoundException(string id) : Exception($"Account '{id}' was not found.");
