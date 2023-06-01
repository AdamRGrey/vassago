namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

public class Message
{
    public Guid Id { get; set; }
    public ulong? ExternalId { get; set; }
    public string Content { get; set; }
    public bool MentionsMe { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public bool ActedOn { get; set; }
    ///however it came from the protocol.
    public string ExternalRepresentation { get; set; }
    public IEnumerable<Attachment> Attachments { get; set; }
    public User Author { get; set; }
    public Channel Channel { get; set; }

    public virtual Task Reply(string message)
    {
        throw new NotImplementedException("derive from me");
    }
    public virtual Task React(string reaction)
    {
        throw new NotImplementedException("derive from me");
    }
}
