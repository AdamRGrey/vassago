namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using static vassago.Models.Enumerations;

public abstract class ProtocolConfiguration
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Protocol { get; set; }
    public Channel SelfChannel { get; set; }
}
public class ProtocolTwitch : ProtocolConfiguration
{
    public string username { get; set; }
    public string oauth { get; set; }
    public ProtocolTwitch() { this.Protocol = "twitch"; }
}
public class ProtocolDiscord : ProtocolConfiguration
{
    public string token { get; set; }

    ///<summary>
    /// discord doesn't like prefixed commands, they want you to use slash commands.
    ///because they don't like bots in their chat channels, they want *a p p s* on their *p l a t f o r m*.
    ///cue bloviating rhetoric that pretends it in any way does anything for privacy or security,
    ///rather than solely increasing the difficulty of swapping them for less shitty owners.
    ///then of course, just like everyone else, they made a useless piece of shit chatbot (Clyde) and shoved it down everyone's throat.
    ///shitheels.
    ///</summary>
    public bool SetupSlashCommands { get; set; } = false;
    public ProtocolDiscord() { this.Protocol = "discord"; }
}
public class ProtocolExternal : ProtocolConfiguration
{
    public ProtocolExternal() { this.Protocol = "external"; }
    public string ExternalId { get; set; }
    ///<summary>
    ///rest - you tell me when you update, you call me for instructions. i am RESTful.
    ///webhooks - like rest, but I call you when you need instructions.
    ///websocket - like webhooks, but a websocket. "no duh, adam. Why though?" faster than restful calls.
    ///            I assume. i don't even know tbh. also, to paraphrase a pretty good youtuber: "don't bother, it'll scale fine with your zero users"
    ///</summary>
    public ExternalProtocolStyle Style { get; set; }
    // public Uri HookReply { get; set; } //TODO: webhook style
    // public Uri HookSendMessage { get; set; }
    // public Uri HookDie { get; set; }
    // public Uri HookSendFile { get; set; }
    // public Uri HookReact { get; set; }
    // public Uri HookUpdateConfiguration { get; set; }
    // TODO: websocket style
    // TODO: kafka style
}
