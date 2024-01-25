namespace vassago.Behavior;

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class TwitchSummon : Behavior
{
    public override string Name => "Twitch Summon";

    public override string Trigger => "!twitchsummon";

    //TODO: Permission! anyone can summon from anywhere... anyone can summon to themselves.
    //I think given the bot's (hopeful) ability to play nice with others - anyone can summon it anywhere
    //HOWEVER, if not-the-broadcaster summons it, 1) all channel permissions to strict and 2) auto-disconnect on stream end
    //i don't know if the twitch *chat* interface has knowledge of if the stream ends. maybe auto-disconnect after like 2 hours?

    public override bool ShouldAct(Message message)
    {
        return false;
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
