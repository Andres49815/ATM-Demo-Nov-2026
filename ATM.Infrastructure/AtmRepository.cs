using ATM.Application.Persistence;
using ATM.Domain;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure;

internal sealed class AtmRepository(AtmDbContext db) : IAtmRepository
{
    public Task<Account?> FindAccountAsync(string id, CancellationToken cancellationToken) =>
        db.Accounts.SingleOrDefaultAsync(account => account.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Account>> ListAccountsAsync(CancellationToken cancellationToken) =>
        await db.Accounts.AsNoTracking().OrderBy(account => account.Id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Transaction>> ListTransactionsAsync(CancellationToken cancellationToken) =>
        await db.Transactions.AsNoTracking().OrderByDescending(transaction => transaction.Id).ToListAsync(cancellationToken);

    public async Task SaveTransactionAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);
    }
}
