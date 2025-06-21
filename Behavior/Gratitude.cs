namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class Gratitude : Behavior
{
    public override string Name => "Gratitude";

    public override string Trigger => "thank me";

    public override bool ShouldAct(Message message, List<UAC> matchedUACs)
    {
        if(Behaver.Instance.IsSelf(message.Author.Id))
            return false;

        return Regex.IsMatch(message.TranslatedContent, "\\bthank (yo)?u\\b", RegexOptions.IgnoreCase) && message.MentionsMe;
    }
    public override async Task<bool> ActOn(Message message)
    {

        switch (Shared.r.Next(4))
        {
            case 0:
                await Behaver.Instance.SendMessage(message.Channel.Id, "you're welcome, citizen!");
                break;
            case 1:
                await Behaver.Instance.React(message.Id, ":)");
                break;
            case 2:
                Behaver.Instance.React(message.Id, "\U0001F607"); //smiling face with halo
                break;
            case 3:
                switch (Shared.r.Next(9))
                {
                    case 0:
                        await Behaver.Instance.React(message.Id, "<3"); //normal heart, usually rendered red
                        break;
                    case 1:
                        Behaver.Instance.React(message.Id, "\U0001F9E1"); //orange heart
                        break;
                    case 2:
                        Behaver.Instance.React(message.Id, "\U0001F49B"); //yellow heart
                        break;
                    case 3:
                        Behaver.Instance.React(message.Id, "\U0001F49A"); //green heart
                        break;
                    case 4:
                        Behaver.Instance.React(message.Id, "\U0001F499"); //blue heart
                        break;
                    case 5:
                        Behaver.Instance.React(message.Id, "\U0001F49C"); //purple heart
                        break;
                    case 6:
                        Behaver.Instance.React(message.Id, "\U0001F90E"); //brown heart
                        break;
                    case 7:
                        Behaver.Instance.React(message.Id, "\U0001F5A4"); //black heart
                        break;
                    case 8:
                        Behaver.Instance.React(message.Id, "\U0001F90D"); //white heart
                        break;
                }
                break;
        }
        return true;
    }
}
