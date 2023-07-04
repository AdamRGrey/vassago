namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;
using static vassago.Models.Enumerations;

[StaticPlz]
public class DefinitionSnarkCogDiss : Behavior
{
    public override string Name => "Definition Snarkiness: cognitivie dissonance";

    public override string Trigger => "\\bcognitive dissonance";

    public override string Description => "snarkiness about the rampant misuse of the term cognitive dissonance";

    public override bool ShouldAct(Message message)
    {
        if((MeannessFilterLevel)message.Channel.EffectivePermissions.MeannessFilterLevel < MeannessFilterLevel.Medium)
            return false;

        return base.ShouldAct(message);
    }

    public override async Task<bool> ActOn(Message message)
    {
        await message.Reply("that's not what cognitive dissonance means. Did you mean \"hypocrisy\"?");
        return true;
    }
}