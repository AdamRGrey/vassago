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
            Behaver.Instance.SendFile(message.Channel.Id, "assets/ekgblip.png", null);
        else
            Behaver.Instance.SendMessage(message.Channel.Id, "[lub-dub]");
        return true;
    }
}
