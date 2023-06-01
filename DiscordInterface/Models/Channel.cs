namespace vassago.DiscordInterface.Models;

using System.Threading.Tasks;
using vassago.Models;

public class DiscordChannel : Channel
{
    public override Task<Message> SendFile(string path, string messageText = null)
    {
        throw new System.NotImplementedException();
    }

    public override Task<Message> SendMessage(string text)
    {
        
        throw new System.NotImplementedException();
    }
}