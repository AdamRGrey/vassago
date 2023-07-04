namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class WishLuck : Behavior
{
    public override string Name => "wish me luck";

    public override string Trigger => "wish me luck";

    public override string Description => "wishes you luck";

    public override async Task<bool> ActOn(Message message)
    {
        var toSend = "☘️";
        if (Shared.r.Next(20) == 0)
        {
            toSend = "\U0001f340";//4-leaf clover
        }
        if(message.Channel.EffectivePermissions.ReactionsPossible == true)
            await message.React(toSend);
        else
            await message.Channel.SendMessage(toSend);
        return true;
    }
}