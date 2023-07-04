namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class PulseCheck : Behavior
{
    public override string Name => "pulse check";

    public override string Trigger => "!pulse ?check";

    public override async Task<bool> ActOn(Message message)
    {
        if(message.Channel.EffectivePermissions.MaxAttachmentBytes >= 16258)
            await message.Channel.SendFile("assets/ekgblip.png", null);
        else
            await message.Channel.SendMessage("[lub-dub]");
        return true;
    }
}