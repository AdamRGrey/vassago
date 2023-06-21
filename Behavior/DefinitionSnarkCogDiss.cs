namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class DefinitionSnarkCogDiss : Behavior
{
    public override string Name => "Definition Snarkiness: cognitivie dissonance";

    public override string Trigger => "\\bcognitive dissonance";

    public override string Description => "snarkiness about the rampant misuse of the term cognitive dissonance";

    public override async Task<bool> ActOn(Message message)
    {
        await message.Reply("that's not what cognitive dissonance means. Did you mean \"hypocrisy\"?");
        return true;
    }
}