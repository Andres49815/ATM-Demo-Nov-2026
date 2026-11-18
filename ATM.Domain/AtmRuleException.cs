namespace ATM.Domain;

public sealed class AtmRuleException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
