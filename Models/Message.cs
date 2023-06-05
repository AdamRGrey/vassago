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
