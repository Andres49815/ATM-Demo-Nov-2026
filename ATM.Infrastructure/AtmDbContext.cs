using ATM.Domain;
using Microsoft.EntityFrameworkCore;

namespace ATM.Infrastructure;

internal sealed class AtmDbContext(DbContextOptions<AtmDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Account>().HasData(
            new Account("checking", "Checking", 1250m),
            new Account("savings", "Savings", 500m));

        var transaction = model.Entity<Transaction>();
        transaction.Property(t => t.Kind);
        transaction.Property(t => t.Amount);
        transaction.Property(t => t.FromAccountId);
        transaction.Property(t => t.ToAccountId);
        transaction.Property(t => t.OccurredAt);
    }
}
