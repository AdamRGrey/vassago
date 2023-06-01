namespace vassago.DiscordInterface.Models;
using vassago.Models;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

public class DiscordProtocol : Protocol 
{
    public DiscordSocketClient Client {get;set;}

    public override Task<Message> SendFile(string path, string messageText = null)
    {
        throw new System.InvalidOperationException("can't send a file to \"discord\", pick a channel");
    }

    public override Task<Message> SendMessage(string message)
    {
        throw new System.InvalidOperationException("can't send a message to \"discord\", pick a channel");
    }
}