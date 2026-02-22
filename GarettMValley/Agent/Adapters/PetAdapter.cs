using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;

using GarettMValley.Agent.Core;

namespace GarettMValley.Agent.Adapters;

public sealed class PetAdapter : IAgentAdapter
{
    private readonly Pet _pet;
    
    public PetAdapter(Pet monster)
    {
        _pet = monster;
    }
    public AgentId Id => new($"pet:{_pet.Name}:{_pet.GetHashCode()}");
    public string Kind => "pet";
    public GameLocation? Location => _pet.currentLocation;
    public Vector2 Tile => _pet.Tile;
    public Character Character => _pet;
    public object Raw => _pet;

    /// <summary>
    /// Have the Pet face the target tile
    /// </summary>
    /// <param name="targetTile"></param>
    public void FaceTile(Vector2 targetTile) => _pet.faceGeneralDirection(targetTile * 64.0f, 0, false);

    /// <summary>
    /// Pets can't emote, no-op for now.
    /// </summary>
    /// <param name="emoteId"></param>
    public void Emote(int emoteId) {}

    /// <summary>
    /// Pets can't talk, no-op for now.
    /// </summary>
    /// <param name="text"></param>
    public void Say(string text) {}

    /// <summary>
    /// Attempt to have the pets move to the target tile, currently monsters have their own AI; no-op until we create a "boss" brain for a boss battle
    /// </summary>
    /// <param name="targetTile"></param>
    public void TryMoveToward(Vector2 targetTile)
    {
        int dir = MathUtils.DirectionToward(_pet.Tile, targetTile);
        _pet.tryToMoveInDirection(dir, isFarmer: false, damagesFarmer: -1, glider: false);
    }

    /// <summary>
    /// Halt the pet
    /// </summary>
    public void StopMoving()
    {
        _pet.Halt();
        _pet.movementPause = 1;
    }
}