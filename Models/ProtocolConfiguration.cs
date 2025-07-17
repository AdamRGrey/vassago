namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

public abstract class ProtocolConfiguration
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Protocol { get; set; }
    public Channel SelfChannel { get; set; }
}
public class ProtocolTwitch : ProtocolConfiguration
{
    public string username {get; set;}
    public string oauth {get; set;}
    public ProtocolTwitch(){this.Protocol = "twitch";}
}
public class ProtocolDiscord : ProtocolConfiguration
{
    public string token {get;set;}

    ///<summary>
    /// discord doesn't like prefixed commands, they want you to use slash commands.
    ///because they don't like bots in their chat channels, they want *a p p s* on their *p l a t f o r m*.
    ///cue bloviating rhetoric that pretends it in any way does anything for privacy or security,
    ///rather than solely increasing the difficulty of swapping them for less shitty owners.
    ///then of course, just like everyone else,they made a useless piece of shit chatbot (Clyde) and shoved it down everyone's throat.
    ///shitheels.
    ///</summary>
    public bool SetupSlashCommands { get; set; } = false;
    public ProtocolDiscord(){this.Protocol = "discord";}
}
