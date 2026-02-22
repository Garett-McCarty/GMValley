
using StardewModdingAPI;

namespace GarettMValley.Cheats;

internal sealed class CheatManager
{
    private readonly IMonitor _monitor;
    private readonly Dictionary<string, ICheat> _cheats = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _enabled = new(StringComparer.OrdinalIgnoreCase);

    public CheatManager(IMonitor monitor)
    {
        _monitor = monitor;
    }

    /// <summary>
    /// Return all cheats registered ordered alphabetically by name.
    /// </summary>
    public IEnumerable<ICheat> All => _cheats.Values.OrderBy(c => c.Name);

    /// <summary>
    /// Register a cheat into our cheat system.
    /// </summary>
    /// <param name="cheat"></param>
    public void Register(ICheat cheat)
    {
        _cheats[cheat.Id] = cheat;
    }

    public bool TryRun(string id, CheatContext context, string[] args, out string? error)
    {
        error = null;
        if (!_cheats.TryGetValue(id, out var cheat))
        {
            error = $"Unknown cheat '{id}'.";
            return false;
        }

        if (!cheat.CanRun(context, out var reason))
        {
            error = reason ?? "Cheat cannot run right now.";
            return false;
        }

        if (cheat.IsToggle)
        {
            if (_enabled.Contains(id))
                _enabled.Remove(id);
            else
                _enabled.Add(id);
        }

        cheat.Run(context, args);
        return true;
    }

    public bool IsEnabled(string id) => _enabled.Contains(id);
}