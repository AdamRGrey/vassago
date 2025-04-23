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
        myUAC = Rememberer.SearchUAC(uac => uac.OwnerId == uacID);
        if(myUAC == null)
        {
            myUAC = new()
            {
                OwnerId = uacID,
                DisplayName = Name
            };
            Rememberer.RememberUAC(myUAC);
        }
    }
    public override bool ShouldAct(Message message)
    {
        if (!base.ShouldAct(message))
            return false;
        var uacConf = Rememberer.SearchUAC(uac => uac.OwnerId == uacID);
        if (uacConf == null)
        {
            Console.Error.WriteLine("no UAC conf for TwitchSummon! Set one up!");
        }

        Console.WriteLine($"uacConf: {uacConf} users: {uacConf?.Users?.Count()}. message author: {message?.Author}. has an IsUser: {message?.Author?.IsUser}.");
        Console.WriteLine($"and therefore: {uacConf.Users.Contains(message.Author.IsUser)}");
        return uacConf.Users.Contains(message.Author.IsUser);
    }

    public override async Task<bool> ActOn(Message message)
    {
        var ti = ProtocolInterfaces.ProtocolList.twitchs.FirstOrDefault();
        if(ti != null)
        {
            var channelTarget = message.Content.Substring(message.Content.IndexOf(Trigger) + Trigger.Length + 1).Trim();
            await message.Channel.SendMessage(ti.AttemptJoin(channelTarget));
        }
        else
        {
            await message.Reply("i don't have a twitch interface running :(");
        }
        return true;
    }
}
