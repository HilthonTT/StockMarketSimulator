namespace StockMarketSimulator.Api.Infrastructure.Database;

public sealed class DatabaseInitializationCompletionSignal
{
    private readonly TaskCompletionSource _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task WaitForInitializationAsync() => _completionSource.Task;

    public void MarkInitializationComplete() => _completionSource.TrySetResult();
}
