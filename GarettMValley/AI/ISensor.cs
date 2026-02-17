using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace GarettMValley.AI;

/// <summary>
/// ISensor provided perception for Agents
/// </summary>
public interface ISensor
{
    void Sense(Blackboard blackboard);
}