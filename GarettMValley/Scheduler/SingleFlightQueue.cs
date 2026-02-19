using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace GarettMValley.Scheduler;

internal sealed class SingleFlightQueue : IDisposable
{
    private readonly IMonitor _monitor;
    private readonly Channel<WorkItem> _channel;
    private readonly CancellationTokenSource _shutdownCts = new();
    private readonly Task _worker;
    private CancellationTokenSource? _currentJobCts;

    private sealed record WorkItem(
        Func<CancellationToken, Task> Run,
        bool ReplaceQueue
    );

    public SingleFlightQueue(IMonitor monitor)
    {
        _monitor = monitor;
        _channel = Channel.CreateUnbounded<WorkItem>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });
        _worker = Task.Run(WorkerLoop);
    }

    public void Dispose()
    {
        try
        {
            _shutdownCts.Cancel();
            _currentJobCts?.Cancel();
        } catch { /* ignore */ }

        _channel.Writer.TryComplete();

        try
        {
            _worker.Wait(TimeSpan.FromSeconds(2));
        } catch { /* ignore */ }

        _shutdownCts.Dispose();
        _currentJobCts?.Dispose();
    }

    /// <summary>
    /// Enqueue a job. Jobs run in FIFO order, one at a time.
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public bool Enqueue(Func<CancellationToken, Task> job)
    {
        return _channel.Writer.TryWrite(new WorkItem(job, ReplaceQueue: false));
    }

    /// <summary>
    /// Cancel the current job and discard anything queued; then run this job next.
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public bool ReplaceCurrent(Func<CancellationToken, Task> job)
    {
        return _channel.Writer.TryWrite(new WorkItem(job, ReplaceQueue: true));
    }

    /// <summary>
    /// Cancel whatever is running right now.
    /// </summary>
    public void CancelCurrent()
    {
        try
        {
            _currentJobCts?.Cancel();
        } catch {}
    }

    private async Task WorkerLoop()
    {
        while(!_shutdownCts.IsCancellationRequested)
        {
            WorkItem item;

            try
            {
                item = await _channel.Reader.ReadAsync(_shutdownCts.Token).ConfigureAwait(false);
            } catch (OperationCanceledException)
            {
                break;
            } catch (ChannelClosedException)
            {
                break;
            }

            if (item.ReplaceQueue)
            {
                CancelCurrent();
                DrainQueue();
            }

            _currentJobCts?.Dispose();
            _currentJobCts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownCts.Token);

            try
            {
                await item.Run(_currentJobCts.Token).ConfigureAwait(false);
            } catch(OperationCanceledException) {}
            catch (Exception exception)
            {
                _monitor.Log($"Queue job failed: {exception}", LogLevel.Error);
            }
        }
    }

    private void DrainQueue()
    {
        while (_channel.Reader.TryRead(out _)) { /* discard */ }
    }
}