using System;
using StardewValley;
using StardewValley.Menus;

namespace GarettMValley.AI;

internal static class DialogueUI
{
    public static void ShowLineThenOptions(NPC npc, GameLocation location, string line, string[] playerOptions, Action<int, string> onPicked)
    {
        if (location is null | npc is null)
            return;
        if (playerOptions is null || playerOptions.Length != 3)
            return;
        
        Response[] responses = new Response[3]
        {
            new Response("npc_option_0", playerOptions[0]),
            new Response("npc_option_1", playerOptions[1]),
            new Response("npc_option_2", playerOptions[2])
        };

        location?.createQuestionDialogue(
            question: line,
            answerChoices: responses,
            afterDialogueBehavior: (Farmer who, string answer) =>
            {
                int idx = answer switch
                {
                    "npc_option_0" => 0,
                    "npc_option_1" => 1,
                    "npc_option_2" => 2,
                    _ => -1,
                };

                if (idx >= 0)
                    onPicked(idx, playerOptions[idx]);
            },
            speaker: npc
        );
    }
}