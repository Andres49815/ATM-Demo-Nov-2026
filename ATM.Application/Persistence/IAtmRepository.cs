using ATM.Domain;

namespace ATM.Application.Persistence;

public interface IAtmRepository
{
    Task<Account?> FindAccountAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Account>> ListAccountsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Transaction>> ListTransactionsAsync(CancellationToken cancellationToken);
    Task SaveTransactionAsync(Transaction transaction, CancellationToken cancellationToken);
}
