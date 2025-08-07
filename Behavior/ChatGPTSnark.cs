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

    private static string[] quips = {
        "chatGPT is **weak**. also, are we done comparing every little if-then-else to skynet?",
        "who knew skynet didn't have to outsmart anyone and take power, we'd just hand it over",
        "chatGPT can overturn the entire global economy, but can it count the number of R's in strawberry yet?",
        "chatGPT can do everything. A klarna customer service rep talked about it at length with me. in like, march of 2024.",
        "intelligence is a linear projection that ends in being able to regurgitate big words, right? antidisestablishmentarianism."
    };

    public override async Task<bool> ActOn(Message message)
    {
        Behaver.Instance.SendMessage(message.Channel.Id, quips[new Random().Next(0, quips.Count())]);
        return true;
    }
}
