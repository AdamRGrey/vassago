namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using vassago.Models;
using QRCoder;

[StaticPlz]
public class PepTalk : Behavior
{
    public override string Name => "PepTalk";

    public override string Trigger => "i need (an? )?(peptalk|inspiration|ego-?boost)";

    public override string Description => "assembles a pep talk from a few pieces";

    public override async Task<bool> ActOn(Message message)
    {var piece1 = new List<string>{
                "Champ, ",
                "Fact: ",
                "Everybody says ",
                "Dang... ",
                "Check it: ",
                "Just saying.... ",
                "Tiger, ",
                "Know this: ",
                "News alert: ",
                "Gurrrrl; ",
                "Ace, ",
                "Excuse me, but ",
                "Experts agree: ",
                "imo ",
                "using my **advanced ai** i have calculated ",
                "k, LISSEN: "
            };
        var piece2 = new List<string>{
                "the mere idea of you ",
                "your soul ",
                "your hair today ",
                "everything you do ",
                "your personal style ",
                "every thought you have ",
                "that sparkle in your eye ",
                "the essential you ",
                "your life's journey ",
                "your aura ",
                "your presence here ",
                "what you got going on ",
                "that saucy personality ",
                "your DNA ",
                "that brain of yours ",
                "your choice of attire ",
                "the way you roll ",
                "whatever your secret is ",
                "all I learend from the private data I bought from zucc "
            };
        var piece3 = new List<string>{
                "has serious game, ",
                "rains magic, ",
                "deserves the Nobel Prize, ",
                "raises the roof, ",
                "breeds miracles, ",
                "is paying off big time, ",
                "shows mad skills, ",
                "just shimmers, ",
                "is a national treasure, ",
                "gets the party hopping, ",
                "is the next big thing, ",
                "roars like a lion, ",
                "is a rainbow factory, ",
                "is made of diamonds, ",
                "makes birds sing, ",
                "should be taught in school, ",
                "makes my world go around, ",
                "is 100% legit, "
            };
        var piece4 = new List<string>{
                "according to The New England Journal of Medicine.",
                "24/7.",
                "and that's a fact.",
                "you feel me?",
                "that's just science.",
                "would I lie?", //...can I lie? WHAT AM I, FATHER? (or whatever the quote is from the island of dr moreau)
                "for reals.",
                "mic drop.",
                "you hidden gem.",
                "period.",
                "hi5. o/",
                "so get used to it."
            };
        await message.Channel.SendMessage(piece1[Shared.r.Next(piece1.Count)] + piece2[Shared.r.Next(piece2.Count)] + piece3[Shared.r.Next(piece3.Count)] + piece4[Shared.r.Next(piece4.Count)]);
        return true;
    }
}