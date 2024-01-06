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
            await message.Channel.SendMessage("I don't know any. Adam!");
        }
        var thisJoke = jokes[Shared.r.Next(jokes.Length)];
        if (thisJoke.Contains("?") && !thisJoke.EndsWith('?'))
        {
            #pragma warning disable 4014
            Task.Run(async () =>
            {
                var firstIndexAfterQuestionMark = thisJoke.LastIndexOf('?') + 1;
                var straightline = thisJoke.Substring(0, firstIndexAfterQuestionMark);
                var punchline = thisJoke.Substring(firstIndexAfterQuestionMark, thisJoke.Length - firstIndexAfterQuestionMark).Trim();
                Task.WaitAll(message.Channel.SendMessage(straightline));
                Thread.Sleep(TimeSpan.FromSeconds(Shared.r.Next(5, 30)));
                if (message.Channel.EffectivePermissions.ReactionsPossible == true && Shared.r.Next(8) == 0)
                {
                    Behaver.Behaviors.Add(new LaughAtOwnJoke(punchline));
                }
                await message.Channel.SendMessage(punchline);
                // var myOwnMsg = await message.Channel.SendMessage(punchline);
            });
            #pragma warning restore 4014
        }
        else
        {
            await message.Channel.SendMessage(thisJoke);
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
        if(Behaver.Instance.IsSelf(message.Author.Id))
            return false;

        Console.WriteLine($"{message.Content} == {_punchline}");
        return message.Content == _punchline
        && Behaver.Instance.IsSelf(message.Author.Id);
    }

    public override async Task<bool> ActOn(Message message)
    {
        await message.React("\U0001F60E"); //smiling face with sunglasses
        Behaver.Behaviors.Remove(this);
        return true;
    }
}