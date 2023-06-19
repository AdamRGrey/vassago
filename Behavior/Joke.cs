namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;

public class Joke : Behavior
{
    public override string Name => "Joke";

    public override string Trigger => "!joke";

    public override string Description => "tell a joke";

    public override async Task<bool> ActOn(PermissionSettings permissions, Message message)
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
                var punchline = thisJoke.Substring(firstIndexAfterQuestionMark, thisJoke.Length - firstIndexAfterQuestionMark);
                Task.WaitAll(message.Channel.SendMessage(straightline));
                Thread.Sleep(TimeSpan.FromSeconds(Shared.r.Next(5, 30)));
                await message.Channel.SendMessage(punchline);
                // var myOwnMsg = await message.Channel.SendMessage(punchline);
                if (Shared.r.Next(8) == 0)
                {
                    LaughAtOwnJoke.punchlinesAwaitingReaction.Add(punchline);                     
                }
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