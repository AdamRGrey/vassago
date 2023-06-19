namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;

public class LaughAtOwnJoke : Behavior
{
    public override string Name => "Laugh at own jokes";

    public override string Trigger => "1 in 8";

    public override string Description => Name;
    public static List<string> punchlinesAwaitingReaction = new List<string>();

    public override bool ShouldAct(PermissionSettings permissions, Message message)
    {
        //TODO: i need to keep track of myself from here somehow
        return false;
        //return message.Author == me && punchlinesAwaitingReaction.Contains(message.Content);
    }

    public override async Task<bool> ActOn(PermissionSettings permissions, Message message)
    {
        punchlinesAwaitingReaction.Remove(message.Content);
        await message.React("\U0001F60E"); //smiling face with sunglasses
        return true;
    }
}