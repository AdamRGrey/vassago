namespace vassago.Behavior;

using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class RoomRead : Behavior
{
    public override string Name => "Room Read";

    public override string Trigger => "!roomread";

    public override async Task<bool> ActOn(Message message)
    {
        var sb = new StringBuilder();
        sb.Append("Channel owned by: ");
        sb.Append("🤷");
        sb.Append(". Meanness level: ");
        sb.Append(message.Channel.EffectivePermissions.MeannessFilterLevel.GetDescription());
        sb.Append(". Lewdness level: ");
        sb.Append(message.Channel.EffectivePermissions.LewdnessFilterLevel.GetDescription());
        sb.Append(".");
        Behaver.Instance.SendMessage(message.Channel.Id, sb.ToString());
        return true;
    }
}
