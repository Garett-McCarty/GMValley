namespace GarettMValley.Cheats.Cheats;

internal sealed class ToggleDialogueCheat : ICheat
{
    public string Id => "dialogue";
    public string Name => "Toggle AI dialogue takeover";
    public string Description => "Enables/disables the DialogueSystem takeover (same as F12).";
    public bool IsToggle => true;

    public bool CanRun(CheatContext context, out string? reason)
    {
        if (context.dialogue is null)
        {
            reason = "Dialogue system is not available.";
            return false;
        }
        reason = null;
        return true;
    }

    public void Run(CheatContext context, string[] args)
    {
        context.dialogue!.Enabled = !context.dialogue.Enabled;
        context.monitor.Log($"DialogueSystem Enabled={context.dialogue.Enabled}", StardewModdingAPI.LogLevel.Info);
    }
}
