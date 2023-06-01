namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Channel
{
    public Guid Id { get; set; }
    public ulong? ExternalId { get; set; }
    public string DisplayName { get; set; }
    public bool IsDM { get; set; }
    public IEnumerable<User> OtherUsers { get; set; }
    public PermissionSettings PermissionsOverrides { get; set; }
    public IEnumerable<Channel> SubChannels { get; set; }
    public Channel ParentChannel { get; set; }
    public Protocol Protocol { get; set; }
    public IEnumerable<Message> Messages { get; set; }

    public virtual Task<Message> SendMessage(string text)
    {
        throw new NotImplementedException("derive from me");
    }
    public virtual Task<Message> SendFile(string path, string messageText = null)
    {
        throw new NotImplementedException("derive from me");
    }
}
