namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;


public class LaughAtOwnJoke : Behavior
{
    public override string Name => "Laugh at own jokes";

    public override string Trigger => "1 in 8";

    public override string Description => Name;
    private string _punchline{get;set;}

    public LaughAtOwnJoke(string punchline)
    {
        _punchline = punchline;
    }
    public override bool ShouldAct(Message message)
    {
        Console.WriteLine($"{message.Content} == {_punchline}");
        return message.Content == _punchline
        && Behaver.Instance.Selves.Any(acc => acc.Id == message.Author.Id);
    }

    public override async Task<bool> ActOn(Message message)
    {
        await message.React("\U0001F60E"); //smiling face with sunglasses
        Behaver.Behaviors.Remove(this);
        return true;
    }
}