namespace vassago.Models;

using System;
using System.Collections.Generic;

public class User //more like "user's account - no concept of the person outside of the protocol. (yet?)
{
    public Guid Id { get; set; }
    public ulong? ExternalId { get; set; }
    public string DisplayName { get; set; }
    public bool IsBot { get; set; } //webhook counts
    public IEnumerable<Channel> SeenInChannels { get; set; }
    public IEnumerable<User> KnownAliases { get; set; }
    public Protocol Protocol { get; set; }
    public string External { get; set; }
}