namespace ATM.Domain;

public enum TransactionKind
{
    Deposit,
    Withdrawal,
    Transfer
}

public sealed class Transaction(
    TransactionKind kind, decimal amount, string? fromAccountId, string? toAccountId, DateTimeOffset occurredAt)
{
    public long Id { get; private set; }
    public TransactionKind Kind { get; } = kind;
    public decimal Amount { get; } = amount;
    public string? FromAccountId { get; } = fromAccountId;
    public string? ToAccountId { get; } = toAccountId;
    public DateTimeOffset OccurredAt { get; } = occurredAt;
}
