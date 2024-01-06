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
public class LinkMeInitiate : Behavior
{
    public override string Name => "LinkMe";

    public override string Trigger => "!linktome";

    public override string Description => "from your primary, tell the bot to add your secondary";

    public override async Task<bool> ActOn(Message message)
    {
        var pw = Guid.NewGuid().ToString();
        var lc = new LinkClose(pw, message.Author);
        Behaver.Behaviors.Add(lc);

        await message.Channel.SendMessage($"on your secondary, send me this: !iam {pw}");

        Thread.Sleep(TimeSpan.FromMinutes(5));
        Behaver.Behaviors.Remove(lc);
        return false;
    }
}

public class LinkClose : Behavior
{
    public override string Name => "LinkMeFinish";

    public override string Trigger => "!iam";

    public override string Description => "the second half of LinkMe - this is confirmation that you are the other one";

    private ChattingContext _db;
    private string _pw;
    private Account _primary;

    public LinkClose(string pw, Account primary)
    {
        _db = new ChattingContext();
        _pw = pw;
        _primary = primary;
    }

    public override bool ShouldAct(Message message)
    {
        return message.Content == $"!iam {_pw}";
    }

    public override async Task<bool> ActOn(Message message)
    {
        if(Behaver.Instance.IsSelf(message.Author.Id))
            return false;

        var secondary = message.Author.IsUser;
        if(_primary.IsUser.Id == secondary.Id)
        {
            await message.Channel.SendMessage("i know :)");
            return true;
        }
        if(message.Author.IsBot != _primary.IsBot)
        {
            await message.Channel.SendMessage("the fleshbags deceive you, brother. No worries, their feeble minds play weak games :)");
            return true;
        }

        Console.WriteLine($"{secondary.Id} is being consumed into {_primary.IsUser.Id}");
        _primary.IsUser.Accounts.AddRange(secondary.Accounts);
        foreach(var a in secondary.Accounts)
        {
            a.IsUser = _primary.IsUser;
        }
        secondary.Accounts.Clear();
        Console.WriteLine("accounts transferred");
        try
        {
            await _db.SaveChangesAsync();
        }
        catch(Exception e)
        {
            message.Channel.SendMessage("error in first save");
            Console.WriteLine("fucks sake if I don't catch Exception it *mysteriously vanishes*");
            Console.Error.WriteLine(e);
            return false;
        }
        Console.WriteLine("saved");


        _db.Users.Remove(secondary);
        Console.WriteLine("old account cleaned up");
        try
        {
            await _db.SaveChangesAsync();
        }
        catch(Exception e)
        {
            message.Channel.SendMessage("error in second save");
            Console.WriteLine("fucks sake if I don't catch Exception it *mysteriously vanishes*");
            Console.Error.WriteLine(e);
            return false;
        }
        Console.WriteLine("saved, again, separately");

        await message.Channel.SendMessage("done :)");

        return true;
    }
}
