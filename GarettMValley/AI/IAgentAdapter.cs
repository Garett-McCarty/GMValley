using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace GarettMValley.AI;

/// <summary>
/// Interface for an AgentAdapter
/// </summary>
public interface IAgentAdapter
{
    /// <summary>
    /// Agent Identifier
    /// </summary>
    AgentId Id { get; }

    /// <summary>
    /// Agent kind name
    /// </summary>
    string Kind { get; }

    /// <summary>
    /// Agents GameLocation
    /// </summary>
    GameLocation? Location { get; }

    /// <summary>
    /// Agents Position Tile
    /// </summary>
    Vector2 Tile { get; }

    Character Character { get; }
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