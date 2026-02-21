using Microsoft.Xna.Framework;
using StardewValley;

namespace GarettMValley.Agent;

/// <summary>
/// Interface for an AgentAdapter
/// </summary>
public interface IAgentAdapter
{
    /// <summary>
    /// Unique identifier for the agent
    /// </summary>
    AgentId Id { get; }

    /// <summary>
    /// Agent kind identifier
    /// </summary>
    string Kind { get; }

    /// <summary>
    /// Agents map location
    /// </summary>
    GameLocation? Location { get; }

    /// <summary>
    /// Agents position tile
    /// </summary>
    Vector2 Tile { get; }

    /// <summary>
    /// Agents physical character
    /// </summary>
    Character Character { get; }

    /// <summary>
    /// Get the raw reference to our adapter
    /// </summary>
    object Raw { get; }

    /// <summary>
    /// Have the agent face a given tile in game
    /// </summary>
    /// <param name="targetTile"></param>
    void FaceTile(Vector2 targetTile);

    /// <summary>
    /// Have the agent emote in game
    /// </summary>
    /// <param name="emoteId"></param>
    void Emote(int emoteId);

    /// <summary>
    /// Have the agent say text in game
    /// </summary>
    /// <param name="text"></param>
    void Say(string text);

    /// <summary>
    /// Have the agent try to move to a given tile
    /// </summary>
    /// <param name="targetTile"></param>
    void TryMoveToward(Vector2 targetTile);

    /// <summary>
    /// Have the agent stop moving
    /// </summary>
    void StopMoving();
}