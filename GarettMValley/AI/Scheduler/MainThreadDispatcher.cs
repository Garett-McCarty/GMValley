using System;
using System.Collections.Concurrent;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace GarettMValley;

internal sealed class MainThreadDispatcher
{
    private readonly ConcurrentQueue<Action> _actions = new();
    private readonly IMonitor _monitor;

    public MainThreadDispatcher(IModHelper helper, IMonitor monitor)
    {
        _monitor = monitor;
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
    }

    public void Enqueue(Action action)
    {
        if (action is null)
            return;
        _actions.Enqueue(action);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        int budget = 50;
        while (budget-- > 0 && _actions.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch(Exception exception)
            {
                _monitor.Log($"Main-thread action failed: {exception}", LogLevel.Error);
            }
        }
    }
}