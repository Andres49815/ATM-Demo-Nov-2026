namespace ATM.Contracts;

public sealed record AccountAmountRequest(string AccountId, decimal Amount);

public sealed record TransferRequest(string FromAccountId, string ToAccountId, decimal Amount);

public sealed record AccountResponse(string Id, string Name, decimal Balance);

public sealed record TransactionResponse(
    long Id, string Kind, decimal Amount, string? FromAccountId, string? ToAccountId, DateTimeOffset OccurredAt);

public sealed record DashboardResponse(
    IReadOnlyList<AccountResponse> Accounts, IReadOnlyList<TransactionResponse> Transactions);
