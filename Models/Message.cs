namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;

public class Message
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public ulong? ExternalId { get; set; }
    public string Content { get; set; }
    /*
    * TODO: more general "talking to me". current impl is platform's capital m Mention, but I'd like it if they use my name without "properly" 
    * mentioning me, and also if it's just me and them in a channel
    */
    public bool MentionsMe { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public bool ActedOn { get; set; }
    public List<Attachment> Attachments { get; set; }
    public User Author { get; set; }
    public Channel Channel { get; set; }


    
    [NonSerialized]
    public Func<string, Task> Reply;

    [NonSerialized]
    public Func<string, Task> React;
}
