
using StardewModdingAPI;

namespace GarettMValley.Cheats.Cheats;

/// <summary>
/// Clear cached agents
/// </summary>
internal sealed class ClearAgentsCheat : ICheat
{
    public string Id => "clear_agents";
    public string Name => "Clear Cached Agents";
    public string Description => "Clears the agent runtime cache. Useful if something gets into a weird state.";
    public bool IsToggle => false;

    public bool CanRun(CheatContext context, out string? reason)
    {
        reason = null;
        return true;
    }

    public void Run(CheatContext context, string[] args)
    {
        context.agents.ClearAgents();
        context.monitor.Log("Cleared agent runtimes.", LogLevel.Info);
    }
}