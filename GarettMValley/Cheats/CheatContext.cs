
using GarettMValley.Agent;
using GarettMValley.Dialogue;
using StardewModdingAPI;
using StardewValley;

namespace GarettMValley.Cheats;

/// <summary>
/// Context given to a cheat.
/// </summary>
internal sealed class CheatContext
{
    public IMonitor monitor { get; }
    public IModHelper helper { get; }
    public ModConfig config { get; }
    public Farmer player => Game1.player;
    public AgentManager agents { get; }
    public DialogueSystem? dialogue { get; }

    public CheatContext(IMonitor monitor, IModHelper helper, ModConfig config, AgentManager agents, DialogueSystem? dialogue)
    {
        this.monitor = monitor;
        this.helper = helper;
        this.config = config;
        this.agents = agents;
        this.dialogue = dialogue;
    }
}