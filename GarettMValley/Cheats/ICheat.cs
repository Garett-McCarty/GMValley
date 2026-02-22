
namespace GarettMValley.Cheats;

internal interface ICheat
{
    /// <summary>
    /// Unique Identifier for cheats
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Display name for the given cheat.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description for the cheat.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Track if we should display this cheat as a togglable (enabled/disabled).
    /// </summary>
    bool IsToggle { get; }

    bool CanRun(CheatContext context, out string? reason);
    void Run(CheatContext context, string[] args);
}