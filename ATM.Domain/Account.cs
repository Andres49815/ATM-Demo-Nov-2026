namespace ATM.Domain;

public sealed class Account(string id, string name, decimal openingBalance)
{
    // Below 2^46 dollars, adjacent cent values stay distinct as JavaScript numbers.
    private const decimal MaximumBalance = 70_368_744_177_663.99m;

    public string Id { get; private set; } = id;
    public string Name { get; private set; } = name;
    public decimal Balance { get; private set; } = openingBalance >= 0
        && openingBalance <= MaximumBalance && decimal.Round(openingBalance, 2) == openingBalance
        ? openingBalance
        : throw new ArgumentOutOfRangeException(nameof(openingBalance));

    private Account() : this("", "", 0) { }

    public void Deposit(decimal amount)
    {
        ValidateAmount(amount);
        EnsureCanCredit(amount);
        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        ValidateAmount(amount);
        EnsureCanDebit(amount);
        Balance -= amount;
    }

    public void TransferTo(Account destination, decimal amount)
    {
        if (Id == destination.Id)
            throw new AtmRuleException("same_account", "Choose two different accounts for a transfer.");

        ValidateAmount(amount);
        EnsureCanDebit(amount);
        destination.EnsureCanCredit(amount);

        Balance -= amount;
        destination.Balance += amount;
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new AtmRuleException("invalid_amount", "Enter a positive amount with at most two decimal places.");
    }

    private void EnsureCanCredit(decimal amount)
    {
        if (amount > MaximumBalance - Balance)
            throw new AtmRuleException("balance_limit", "The resulting balance is too large to represent in cents.");
    }

    private void EnsureCanDebit(decimal amount)
    {
        if (amount > Balance)
            throw new AtmRuleException("insufficient_funds", "This account does not have enough funds.");
    }
}
