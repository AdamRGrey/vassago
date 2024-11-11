namespace vassago.Behavior;

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class TwitchSummon : Behavior
{
    public override string Name => "Twitch Summon";

    public override string Trigger => "!twitchsummon";

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
