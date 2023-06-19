namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;

public class PulseCheck : Behavior
{
    public override string Name => "pulse check";

    public override string Trigger => "!pluse ?check";

    public override async Task<bool> ActOn(PermissionSettings permissions, Message message)
    {
        await message.Channel.SendFile("assets/ekgblip.png", null);
        return true;
    }
}