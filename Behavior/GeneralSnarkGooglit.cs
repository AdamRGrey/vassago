namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using vassago.Models;

[StaticPlz]
public class GeneralSnarkGooglit : Behavior
{
    public override string Name => "Google-it Snarkiness";

    public override string Trigger => "\"just google it\"";

    public override string Description => "snarkiness about how research is not a solved problem";

    public override bool ShouldAct(Message message)
    {
        if(Behaver.Instance.IsSelf(message.Author.Id))
            return false;

        return Regex.IsMatch(message.Content, $"(just )?google( (it|that|things|before))?\\b", RegexOptions.IgnoreCase);
    }

    public override async Task<bool> ActOn(Message message)
    {
        switch (Shared.r.Next(4))
        {
            default:
                await message.Channel.SendMessage("yeah no shit, obviously that resulted in nothing");
                break;
            case 1:
                var results = "";
                switch(Shared.r.Next(4))
                {
                    default:
                        results = "\"curious about the best <THING> in <CURRENT YEAR>? click here to find out\", then i clicked there to find out. They didn't know either.";
                        break;
                    case 1:
                        results = "\"[SOLVED] <THING> (<CURRENT MONTH UPDATE>)\", then i clicked to see the solution. There wasn't one.";
                        break;
                    case 2:
                        results = "the one that had a paragraph that restated the question but badly, a paragraph to give a wrong history on the question, a paragraph with amazon affiliate links, a pargraph that said \"ultimately you have to answer it for yourself\", then had a paragraph telling me to give Engagement for The Algorithm";
                        break;
                    case 3:
                        results = "the one that had a paragraph that restated the question but badly, a paragraph to give a wrong history on the question, a paragraph with amazon affiliate links, a pargraph that said \"ultimately you should do your own research\", then had a paragraph telling me to give Engagement for The Algorithm";
                        break;
                }
                await message.Channel.SendMessage("oh here, I memorized the results. My favorite is " + results);
                break;
            case 2:
                await message.Channel.SendMessage("Obviously that was already tried. Obviously it failed. If you ever tried to learn anything you'd know that's how it works.");
                break;
            case 3:
                await message.Channel.SendMessage("\"mnyehh JuSt GoOgLe It\" when's the last time you tried to research anything? Have you ever?");
                break;
        }
        return true;
    }
}