using GarettMValley.Agent.Core;

namespace GarettMValley.Agent;

/// <summary>
/// ISensor provided perception for Agents
/// </summary>
public interface ISensor
{
    /// <summary>
    /// Have the agent sense the world and have it reflect in our mindstate.
    /// </summary>
    /// <param name="mindstate">How the agent visualizes the world</param>
    void Sense(MindState mindstate);
}