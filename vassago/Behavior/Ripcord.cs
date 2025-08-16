namespace vassago.Behavior;

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class Ripcord: Behavior
{
    public override string Name => "Stop Button";

    public override string Trigger => "!ripcord";

    private static Guid uacID = new Guid("e00b0522-5ac1-46f2-b5e8-8b791692a746");
    private static UAC myUAC;

    public Ripcord()
    {
        myUAC = rememberer.SearchUAC(uac => uac.OwnerId == uacID);
        if (myUAC == null)
        {
            myUAC = new()
            {
                OwnerId = uacID,
                DisplayName = Name,
                Description = @"matching this means you can tell the bot to shutdown, now"
            };
        }
        rememberer.RememberUAC(myUAC);
    }
    public override bool ShouldAct(Message message, List<UAC> matchedUACs)
    {
        if (!base.ShouldAct(message, matchedUACs))
            return false;
        return myUAC.Users.Contains(message.Author.IsUser);
    }

    public override async Task<bool> ActOn(Message message)
    {
        Behaver.Instance.SendMessage(message.Channel.Id, "daisy, dai.. sy....");
        Shared.App.StopAsync();
        return true;
    }
}
