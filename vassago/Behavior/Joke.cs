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
public class Joke : Behavior
{
    public override string Name => "Joke";

    public override string Trigger => "!joke";

    public override string Description => "tell a joke";

    public override async Task<bool> ActOn(Message message)
    {
        Console.WriteLine("telling a joke");
        var jokes = rememberer.JokesOverview();
        if (jokes?.Count == 0)
        {
            Behaver.Instance.SendMessage(message.Channel.Id, "I don't know any. Adam!");
            return false;
        }
        jokes = jokes.Where(j => j.LewdnessConformity <= message.Channel.EffectivePermissions.LewdnessFilterLevel &&
                            j.MeannessConformity <= message.Channel.EffectivePermissions.MeannessFilterLevel).ToList();
        if (jokes?.Count == 0)
        {
            Behaver.Instance.SendMessage(message.Channel.Id, "I don't know any *that are appropriate for this channel*. Adam!");
            return false;
        }
        var thisJoke = jokes[Shared.r.Next(jokes.Count)];
        Behaver.Instance.SendMessage(message.Channel.Id, thisJoke.PrimaryText);

        if(!string.IsNullOrWhiteSpace(thisJoke.SecondaryText))
        {
            Thread.Sleep(TimeSpan.FromSeconds(Shared.r.Next(5, 30)));
            if (message.Channel.EffectivePermissions.ReactionsPossible == true && Shared.r.Next(8) == 0)
            {
                Behaver.Behaviors.Add(new LaughAtOwnJoke(thisJoke.SecondaryText));
            }
            Behaver.Instance.SendMessage(message.Channel.Id, thisJoke.SecondaryText);
            // var myOwnMsg = await message.Channel.SendMessage(punchline);
        }
        return true;
    }
}
public class LaughAtOwnJoke : Behavior
{
    public override string Name => "Laugh at own jokes";

    public override string Trigger => "1 in 8";

    public override string Description => Name;
    private string _punchline { get; set; }

    public LaughAtOwnJoke(string punchline)
    {
        _punchline = punchline;
    }
    public override bool ShouldAct(Message message, List<UAC> matchedUACs)
    {
        if (Behaver.Instance.IsSelf(message.Author.Id))
            return false;

        Console.WriteLine($"{message.TranslatedContent} == {_punchline}");
        return message.TranslatedContent == _punchline
        && Behaver.Instance.IsSelf(message.Author.Id);
    }

    public override async Task<bool> ActOn(Message message)
    {
        Behaver.Instance.React(message.Id, "\U0001F60E"); //smiling face with sunglasses
        Behaver.Behaviors.Remove(this);
        return true;
    }
}
