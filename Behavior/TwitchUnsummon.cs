namespace vassago.Behavior;

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class TwitchDismiss : Behavior
{
    public override string Name => "Twitch Dismiss";

    public override string Trigger => "begone, @[me]";

    public override bool ShouldAct(Message message)
    {
        if(message.MentionsMe &&
            (Regex.IsMatch(message.Content.ToLower(), "\\bbegone\\b") || Regex.IsMatch(message.Content.ToLower(), "\\bfuck off\\b")))
            {
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
        var ti = ProtocolInterfaces.ProtocolList.twitchs.FirstOrDefault();

        if(ti != null)
        {
            ti.AttemptLeave(message.Channel.DisplayName);
        }
        else
        {
            await message.Reply("i don't have a twitch interface running :(");
        }
        return true;
    }
}
