namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Discord;

public class Channel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public ulong? ExternalId { get; set; }
    public string DisplayName { get; set; }
    public bool IsDM { get; set; }
    public PermissionSettings PermissionsOverrides { get; set; }
    public List<Channel> SubChannels { get; set; }
    public Channel ParentChannel { get; set; }
    public string Protocol { get; set; }
    public List<Message> Messages { get; set; }

    [NonSerialized]
    public Func<string, string, Task> SendFile;

    [NonSerialized]
    public Func<string, Task> SendMessage;
}