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

        Behaver.Instance.SendMessage(message.Channel.Id, $"on your secondary, send me this: !iam {pw}");

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

    private string _pw;
    private Account _primary;

    public LinkClose(string pw, Account primary)
    {
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

            Behaver.Instance.SendMessage(message.Channel.Id, "i know :)");
            return true;
        }
        if(message.Author.IsBot != _primary.IsBot)
        {
            Behaver.Instance.SendMessage(message.Channel.Id, "the fleshbags deceive you, brother. No worries, their feeble minds play weak games :)");
            return true;
        }

        if(Behaver.Instance.CollapseUsers(_primary.IsUser, secondary))
        {
            Behaver.Instance.SendMessage(message.Channel.Id, "done :)");
        }
        else
        {
            Behaver.Instance.SendMessage(message.Channel.Id, "failed :(");
        }

        return true;
    }
}
