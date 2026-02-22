using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using GarettMValley.Agent.Core;

namespace GarettMValley.Agent.Adapters;

public sealed class VillagerAdapter : IAgentAdapter
{
    private readonly NPC _npc;
    
    public VillagerAdapter(NPC npc)
    {
        _npc = npc;
    }

    public AgentId Id => new($"npc:{_npc.Name}");
    public string Kind => "villager";
    public GameLocation? Location => _npc.currentLocation;
    public Vector2 Tile => _npc.Tile;
    public Character Character => _npc;
    public object Raw => _npc;

    public void FaceTile(Vector2 targetTile) => _npc.faceGeneralDirection(targetTile * 64.0f, 0, false);
    public void Emote(int emoteId) => _npc.doEmote(emoteId);
    public void Say(string text)
    {
        _npc.TemporaryDialogue = new Stack<StardewValley.Dialogue>();
        _npc.TemporaryDialogue.Append(new StardewValley.Dialogue(_npc, "", text));
        StardewValley.Game1.drawDialogue(_npc);
    }
    public void TryMoveToward(Vector2 targetTile)
    {
        _npc.tryToMoveInDirection(MathUtils.DirectionToward(_npc.Tile, targetTile), false, 0, false);
    }
    public void StopMoving()
    {
        _npc.Halt();
        _npc.movementPause = 1;
    }
}