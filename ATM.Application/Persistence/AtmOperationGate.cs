namespace ATM.Application.Persistence;

public sealed class AtmOperationGate : IDisposable
{
    private readonly SemaphoreSlim mutex = new(1, 1);

    // Keep each read or read-modify-save operation consistent within the in-memory ATM.
    public async Task<T> RunAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken)
    {
        await mutex.WaitAsync(cancellationToken);
        try
        {
            return await operation();
        }
        finally
        {
            mutex.Release();
        }
    }

    public void Dispose() => mutex.Dispose();
}
