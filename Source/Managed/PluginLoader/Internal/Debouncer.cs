namespace UnrealEngine.Plugins.Internal;

internal class Debouncer(TimeSpan waitTime) : IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    private readonly TimeSpan _waitTime = waitTime;

    private int _counter;

    public void Execute(Action action)
    {
        int current = Interlocked.Increment(ref _counter);
        _ = Task.Delay(_waitTime).ContinueWith(delegate (Task task)
        {
            if (current == _counter && !_cts.IsCancellationRequested)
            {
                action();
            }
            task.Dispose();
        }, _cts.Token);
    }

    public void Dispose()
    {
        _cts.Cancel();
    }
}
