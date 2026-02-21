using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace GarettMValley.Agent;

/// <summary>
/// ISensor provided perception for Agents
/// </summary>
public interface ISensor
{
    /// <summary>
    /// Have our sensor, sense.
    /// </summary>
    /// <param name="blackboard"></param>
    void Sense(Blackboard blackboard);
}