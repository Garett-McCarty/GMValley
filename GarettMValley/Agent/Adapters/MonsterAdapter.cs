using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace GarettMValley.Agent.Adapters;

public sealed class MonsterAdapter : IAgentAdapter
{
    private readonly Monster _monster;
    
    public MonsterAdapter(Monster monster)
    {
        _monster = monster;
    }
    public AgentId Id => new($"monster:{_monster.Name}:{_monster.GetHashCode()}");
    public string Kind => "monster";
    public GameLocation? Location => _monster.currentLocation;
    public Vector2 Tile => _monster.Tile;

    public Character Character => _monster;
    /// <summary>
    /// Reference to our Raw Game Object
    /// </summary>
    public object Raw => _monster;

    /// <summary>
    /// Have the Monster face the target tile
    /// </summary>
    /// <param name="targetTile"></param>
    public void FaceTile(Vector2 targetTile) => _monster.faceGeneralDirection(targetTile * 64.0f, 0, false);

    /// <summary>
    /// Monsters can't emote, no-op for now.
    /// </summary>
    /// <param name="emoteId"></param>
    public void Emote(int emoteId) {}

    /// <summary>
    /// Monsters can't talk, no-op for now.
    /// </summary>
    /// <param name="text"></param>
    public void Say(string text)
    {
        StardewValley.Game1.addHUDMessage(new StardewValley.HUDMessage(text));
    }

    /// <summary>
    /// Attempt to have the monster move to the target tile, currently monsters have their own AI; no-op until we create a "boss" brain for a boss battle
    /// </summary>
    /// <param name="targetTile"></param>
    public void TryMoveToward(Vector2 targetTile)
    {
        var dir = MathUtils.DirectionToward(_monster.Tile, targetTile);
        SetMovingOnly(dir);
    }

    /// <summary>
    /// Halt the monster
    /// </summary>
    public void StopMoving()
    {
        _monster.Halt();
        _monster.SetMovingUp(false);
        _monster.SetMovingRight(false);
        _monster.SetMovingDown(false);
        _monster.SetMovingLeft(false);
    }

    private void SetMovingOnly(int direction)
    {
        _monster.SetMovingUp(false);
        _monster.SetMovingRight(false);
        _monster.SetMovingDown(false);
        _monster.SetMovingLeft(false);
        switch (direction)
        {
            case 0: _monster.SetMovingUp(true); break;
            case 1: _monster.SetMovingRight(true); break;
            case 2: _monster.SetMovingDown(true); break;
            case 3: _monster.SetMovingLeft(true); break;
        }
    }
}