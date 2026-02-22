using StardewModdingAPI;

namespace GarettMValley.Cheats.Cheats;

internal sealed class RegeneratePersonalitiesCheat : ICheat
{
    public string Id => "regenerate_personalities";
    public string Name => "Regenerate Personalities";
    public string Description => "Rebuilds the personality JSON using current game data.";
    public bool IsToggle => false;
    public bool CanRun(CheatContext context, out string? reason)
    {
        reason = null;
        return true;
    }
    public void Run(CheatContext context, string[] args)
    {
        bool force = true;
        if (args.Length >= 1 && bool.TryParse(args[0], out var parsed))
            force = parsed;

        context.agents.RegeneratePersonalities(force);
    }
}