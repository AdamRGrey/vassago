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
        Console.WriteLine("joking");
        var jokes = File.ReadAllLines("assets/jokes.txt");
        jokes = jokes.Where(l => !string.IsNullOrWhiteSpace(l))?.ToArray();
        if (jokes?.Length == 0)
        {
            Behaver.Instance.SendMessage(message.Channel.Id, "I don't know any. Adam!");
        }
        var thisJoke = jokes[Shared.r.Next(jokes.Length)];
        if (thisJoke.Contains("?") && !thisJoke.EndsWith('?'))
        {
            Task.Run(async () =>
            {
                var firstIndexAfterQuestionMark = thisJoke.LastIndexOf('?') + 1;
                var straightline = thisJoke.Substring(0, firstIndexAfterQuestionMark);
                var punchline = thisJoke.Substring(firstIndexAfterQuestionMark, thisJoke.Length - firstIndexAfterQuestionMark).Trim();
                Task.WaitAll(Behaver.Instance.SendMessage(message.Channel.Id, straightline));
                Thread.Sleep(TimeSpan.FromSeconds(Shared.r.Next(5, 30)));
                if (message.Channel.EffectivePermissions.ReactionsPossible == true && Shared.r.Next(8) == 0)
                {
                    Behaver.Behaviors.Add(new LaughAtOwnJoke(punchline));
                }
                Behaver.Instance.SendMessage(message.Channel.Id, punchline);
                // var myOwnMsg = await message.Channel.SendMessage(punchline);
            });
        }
        else
        {
            Behaver.Instance.SendMessage(message.Channel.Id, thisJoke);
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
    public override bool ShouldAct(Message message)
    {
        if (Behaver.Instance.IsSelf(message.Author.Id))
            return false;

        Console.WriteLine($"{message.Content} == {_punchline}");
        return message.Content == _punchline
        && Behaver.Instance.IsSelf(message.Author.Id);
    }

    public override async Task<bool> ActOn(Message message)
    {
        Behaver.Instance.React(message.Id, "\U0001F60E"); //smiling face with sunglasses
        Behaver.Behaviors.Remove(this);
        return true;
    }
}
