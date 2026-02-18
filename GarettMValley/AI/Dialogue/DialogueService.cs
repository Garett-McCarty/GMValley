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
    private readonly DialogueSystem _dialogue;
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

    public DialogueService(IMonitor monitor, MainThreadDispatcher dispatcher, string ollamaUri, string ollamaModel, int timeoutSeconds, DialogueSystem dialogue)
    {
        _dialogue = dialogue;
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

    public void StartNpcDialogue(NPC npc, string systemPrompt, string userPrompt)
    {
        _queue.ReplaceCurrent(async token =>
        {
            var options = new Dictionary<string, object>
            {
                ["temperature"] = 0.8,
                ["top_p"] = 0.9,
                ["num_predict"] = 120
            };

            // Off-thread network call
            var result = await _ollama.GenerateAsync(
                model: _model,
                prompt: userPrompt,
                system: systemPrompt,
                options: options,
                cancellationToken: token
            ).ConfigureAwait(false);

            string line = SanitizeDialogue(result.Text);

            // Marshal back to the main thread to touch Stardew UI/state
            _dispatcher.Enqueue(() =>
            {
                if (Game1.activeClickableMenu is DialogueBox)
                {
                    Game1.activeClickableMenu = new DialogueBox(line);
                } else
                {
                    DialogueSystem.ShowDialogue(npc, line);
                }
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