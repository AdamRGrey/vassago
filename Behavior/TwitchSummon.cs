namespace vassago.Behavior;

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class TwitchSummon : Behavior
{
    public override string Name => "Twitch Summon";

    public override string Trigger => "!twitchsummon";

    private static Guid uacID = new Guid("06ad2008-3d48-4ba6-8722-7eaea000ec70");
    private static UAC myUAC;

    public TwitchSummon()
    {
        myUAC = rememberer.SearchUAC(uac => uac.OwnerId == uacID);
        if (myUAC == null)
        {
            myUAC = new()
            {
                OwnerId = uacID,
                DisplayName = Name,
                Description = @"matching this means you can summon the bot <i>to</i> <b>any</b> twitch channel"
            };
        }
        rememberer.RememberUAC(myUAC);
    }
    internal static TwitchInterface.TwitchInterface getAnyTwitchInterface()
    {
        return Shared.ProtocolList.FirstOrDefault(ip =>
                                         ip is TwitchInterface.TwitchInterface)
            //.FirstOrDefault()
            as TwitchInterface.TwitchInterface;
    }

    public override bool ShouldAct(Message message, List<UAC> matchedUACs)
    {
        if (!base.ShouldAct(message, matchedUACs))
            return false;

        Console.WriteLine($"myUAC: {myUAC} users: {myUAC?.Users?.Count()}. message author: {message?.Author}. has an IsUser: {message?.Author?.IsUser}.");
        Console.WriteLine($"and therefore: {myUAC.Users.Contains(message.Author.IsUser)}");
        return myUAC.Users.Contains(message.Author.IsUser);
    }

    public override async Task<bool> ActOn(Message message)
    {
        var ti = getAnyTwitchInterface();
        if (ti != null)
        {
            var channelTarget = message.Content.Substring(message.Content.IndexOf(Trigger) + Trigger.Length + 1).Trim();
            Behaver.Instance.SendMessage(message.Channel.Id, ti.AttemptJoin(channelTarget));
        }
        else
        {
            Behaver.Instance.Reply(message.Id, "i don't have a twitch interface running :(");
        }
        return true;
    }
}

[StaticPlz]
public class TwitchDismiss : Behavior
{
    public override string Name => "Twitch Dismiss";

    public override string Trigger => "begone, @[me]";

    public override bool ShouldAct(Message message, List<UAC> matchedUACs)
    {
        var ti = TwitchSummon.getAnyTwitchInterface();
            // Console.WriteLine($"TwitchDismiss checking. menions me? {message.MentionsMe}");
        if (message.MentionsMe &&
            (Regex.IsMatch(message.Content.ToLower(), "\\bbegone\\b") || Regex.IsMatch(message.Content.ToLower(), "\\bfuck off\\b")))
        {
            var channelTarget = message.Content.Substring(message.Content.IndexOf(Trigger) + Trigger.Length + 1).Trim();
            ti.AttemptLeave(channelTarget);
            //TODO: PERMISSION! who can dismiss me? pretty simple list:
            //1) anyone in the channel with authority*
            //2) whoever summoned me
            //* i don't know if the twitch *chat* interface will tell me if someone's a mod.
            return true;
        }
        return false;
    }

    public override async Task<bool> ActOn(Message message)
    {
        var ti = TwitchSummon.getAnyTwitchInterface();

        if (ti != null)
        {
            ti.AttemptLeave(message.Channel.DisplayName);
        }
        else
        {
            Behaver.Instance.Reply(message.Id, "i don't have a twitch interface running :(");
        }
        return true;
    }
}
