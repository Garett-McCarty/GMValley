
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace GarettMValley.Agent;

/// <summary>
/// Config related portion of the AgentManager
/// </summary>
public sealed partial class AgentManager
{
    /// <summary>
    /// Reference to our configuration
    /// </summary>
    private readonly ModConfig _config;
    
    /// <summary>
    /// Get our mod configuration
    /// </summary>
    /// <returns></returns>
    public ModConfig GetConfig() => _config;
}