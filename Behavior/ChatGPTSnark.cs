namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class ChatGPTSnark : Behavior
{
    public override string Name => "ChatGPTSnark";

    public override string Trigger => "chatgpt";

    public override string Description => "snarkiness about the latest culty-fixation in ai";

    public override async Task<bool> ActOn(Message message)
    {
        await message.Channel.SendMessage("chatGPT is **weak**. also, are we done comparing every little if-then-else to skynet?");
        return true;
    }
}