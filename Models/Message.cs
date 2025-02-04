namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

public class Message
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Protocol { get; set; }
    public string ExternalId { get; set; }
    public string Content { get; set; }
    public bool MentionsMe { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public bool ActedOn { get; set; }
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public List<Attachment> Attachments { get; set; }
    public Account Author { get; set; }
    public Channel Channel { get; set; }



    [NonSerialized]
    public Func<string, Task> Reply;

    [NonSerialized]
    public Func<string, Task> React;
}
