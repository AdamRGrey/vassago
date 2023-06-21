namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class DefinitionSnarkGaslight : Behavior
{
    public override string Name => "Definition Snarkiness: gaslighting";

    public override string Trigger => "\\bgaslight(ing)?";

    public override string Description => "snarkiness about the rampant misuse of the term gaslighting";

    public override async Task<bool> ActOn(Message message)
    {
        await message.Channel.SendMessage("that's not what gaslight means. Did you mean \"say something that (you believe) is wrong\"?");
        return true;
    }
}