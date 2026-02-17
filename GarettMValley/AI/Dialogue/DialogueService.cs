using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;

namespace GarettMValley.AI;

internal sealed class DialogueService
{
    /// <summary>
    /// Reference to the monitoring system
    /// </summary>
    private readonly IMonitor _monitor;
    /// <summary>
    /// Reference to the thread dispatcher
    /// </summary>
    private readonly MainThreadDispatcher _dispatcher;
    /// <summary>
    /// Reference to our task queue
    /// </summary>
    private readonly SingleFlightQueue _queue;
    /// <summary>
    /// Reference to our OllamaClient wrapper
    /// </summary>
    private readonly OllamaClient _ollama;
    /// <summary>
    /// Model we want to use with our OllamaClient
    /// </summary>
    private readonly string _model;

    public DialogueService(IMonitor monitor, MainThreadDispatcher dispatcher, string ollamaUri, string ollamaModel, int timeoutSeconds)
    {
        _monitor = monitor;
        _dispatcher = dispatcher;
        _queue = new SingleFlightQueue(monitor);
        _ollama = new OllamaClient(baseUrl: ollamaUri, timeout: TimeSpan.FromSeconds(timeoutSeconds));
        _model = ollamaModel;
    }

    public void Dispose()
    {
        _queue.Dispose();
        _ollama.Dispose();
    }

    public void CancelActive()
    {
        _queue.CancelCurrent();
    }

    public void StartNpcDialogue(NPC npc, string playerIntent, string gameContext)
    {
        string npcName = npc?.Name ?? "Unknown";
        string playerName = Game1.player?.Name ?? "Player";

        _queue.ReplaceCurrent(async token =>
        {
            var system = @"You are a Stardew Valley villager. Stay in character.
Be brief (max 2 sentences), PG. No meta, no quotes, no stage directions.";

            var prompt =
$@"NPC: {npcName}
Player: {playerName}

CURRENT CONTEXT:
{gameContext}

PLAYER INPUT:
{playerIntent}

TASK:
Write ONE in-character Stardew dialogue line (1–2 sentences).";

            var options = new Dictionary<string, object>
            {
                ["temperature"] = 0.8,
                ["top_p"] = 0.9,
                ["num_predict"] = 90
            };

            // Off-thread network call
            var result = await _ollama.GenerateAsync(
                model: _model,
                prompt: prompt,
                system: system,
                options: options,
                cancellationToken: token
            ).ConfigureAwait(false);

            string line = SanitizeDialogue(result.Text);

            // Marshal back to the main thread to touch Stardew UI/state
            _dispatcher.Enqueue(() =>
            {
                // Re-check context: player might have closed menus, etc.
                if (Game1.currentLocation is null || Game1.player is null)
                    return;

                if (string.IsNullOrWhiteSpace(line))
                    line = "…";

                // Show dialogue safely on main thread
                Game1.drawDialogueNoTyping(line);

                // Alternatively:
                // Game1.activeClickableMenu = new DialogueBox(line);
            });
        });
    }

    private static string SanitizeDialogue(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return String.Empty;
        var value = text.Trim();
        if (value.Length > 240)
            value = value.Substring(0, 240).TrimEnd() + "…";
        value = value.Replace("\r", " ").Replace("\n", " ").Trim();
        return value;
    }
}