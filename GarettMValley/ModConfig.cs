
namespace GarettMValley;

/// <summary>
/// Configuration for GarettMValley
/// </summary>
public sealed class ModConfig
{
    /// <summary>
    /// Allow custom dialogue
    /// </summary>
    public bool AllowDialogue { get; set; } = true;

    /// <summary>
    /// Allow NPCs to generate their own dialogue
    /// </summary>
    public bool AllowGeneratedDialogue { get; set; } = true;

    /// <summary>
    /// Allow Villager NPC Agency
    /// </summary>
    public bool AllowVillagerAgency { get; set; } = true;

    /// <summary>
    /// Allow Monster NPC Agency
    /// </summary>
    public bool AllowMonsterAgency { get; set; } = true;

    /// <summary>
    /// Allow Pets NPC Agency
    /// </summary>
    public bool AllowPetsAgency { get; set; } = true;

    /// <summary>
    /// Allow Farm Animal NPC Agency
    /// </summary>
    public bool AllowAnimalAgency { get; set; } = true;

    /// <summary>
    /// Enable Debugging
    /// </summary>
    public bool Debug { get; set; } = true;

    /// <summary>
    /// Enable Debug Logging
    /// </summary>
    public bool DebugLog { get; set; } = true;

    /// <summary>
    /// Enable Ollama Support
    /// </summary>
    public bool EnableOllama { get; set; } = true;

    /// <summary>
    /// Ollama API Url
    /// </summary>
    public string OllamaUrl { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Ollama API requested Model
    /// </summary>
    public string OllamaModel { get; set; } = "gemma2:2b";

    /// <summary>
    /// Ollama Timeout in Seconds
    /// </summary>
    public int OllamaTimeout { get; set; } = 15;

    /// <summary>
    /// Enable the Mod
    /// </summary>
    public bool EnableMod { get; set; } = true;

    /// <summary>
    /// Welcome Message!
    /// </summary>
    public string Message { get; set; } = "Welcome to GarettM Valley! NPC AI Behavior and Agency MOD";
}