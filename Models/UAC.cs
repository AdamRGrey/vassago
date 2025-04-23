namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using vassago.Models;

public class UAC
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    ///<summary indulgence="haiku-like">
    ///behaviors will have
    ///a hardcoded ID thing
    ///so they can find theirs.
    ///</summary>
    public Guid OwnerId { get; set;}
    public string DisplayName { get; set; }
    public List<Account> AccountInChannels { get; set; }
    public List<Channel> Channels { get; set; }
    public List<User> Users { get; set; }
}
