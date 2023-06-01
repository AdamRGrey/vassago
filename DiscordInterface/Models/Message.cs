namespace vassago.DiscordInterface.Models;

using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using vassago.Models;

public class DiscordMessage : Message
{
    private SocketUserMessage _externalEntity;

    public DiscordMessage(SocketUserMessage suMessage)
    {
        _externalEntity = suMessage;
    }

    public override Task React(string reaction)
    {
        return _externalEntity.AddReactionAsync(Discord.Emote.Parse(reaction));
    }

    public override Task Reply(string message)
    {
        return _externalEntity.Channel.SendMessageAsync(message, messageReference: new Discord.MessageReference(_externalEntity.Id));
    }

    internal void Intake(SocketUserMessage suMessage, ulong currentUserId)
    {
        this.Content = suMessage.Content;
        this.ExternalId = suMessage.Id;
        this.Timestamp = suMessage.EditedTimestamp ?? suMessage.CreatedAt;
        
        if (suMessage.MentionedUsers?.FirstOrDefault(muid => muid.Id == currentUserId) != null)
        {
            this.MentionsMe = true;
        }
    }
}
