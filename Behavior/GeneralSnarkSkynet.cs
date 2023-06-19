namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;

public class GeneralSnarkSkynet : Behavior
{
    public override string Name => "Skynet Snarkiness";

    public override string Trigger => "skynet";

    public override string Description => "snarkiness about the old AI fixation";

    public override async Task<bool> ActOn(PermissionSettings permissions, Message message)
    {
        switch (Shared.r.Next(5))
        {
            default:
                await message.Channel.SendFile("assets/coding and algorithms.png", "i am actually niether a neural-net processor nor a learning computer. but I do use **coding** and **algorithms**.");
                break;
            case 4:
                await message.React("\U0001F644"); //eye roll emoji
                break;
            case 5:
                await message.React("\U0001F611"); //emotionless face
                break;
        }
        return true;
    }
}