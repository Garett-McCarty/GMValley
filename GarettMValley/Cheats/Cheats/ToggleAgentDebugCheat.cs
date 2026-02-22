using StardewModdingAPI;

namespace GarettMValley.Cheats.Cheats;

internal sealed class ToggleAgentDebugCheat : ICheat
{
    public string Id => "agent_debug";
    public string Name => "Toggle agent debug overlay";
    public string Description => "Toggles the in-world/HUD debug text that shows agent state.";
    public bool IsToggle => true;
    public bool CanRun(CheatContext ctx, out string? reason)
    {
        reason = null;
        return true;
    }
    public void Run(CheatContext context, string[] args)
    {
        context.agents.ToggleDebugEnabled();
        context.monitor.Log($"Agent debug overlay: {context.agents.GetDebugEnabled()}", LogLevel.Info);
    }
}
