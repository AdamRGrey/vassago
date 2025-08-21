namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

//TODO: rename.
//"uac" originally meant "user account control". but it might just be channel control. in fact, channel-control is much more fun,
//then the platform manages the permissions for you!
//but now I'm going to add locales to it, so it's kind of... "miscellaneous attached data". Official Sticky Notes, if you will.
public class UAC
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    ///<summary indulgence="haiku-like">
    ///behaviors will have
    ///a hardcoded ID thing
    ///so they can find theirs.
    ///</summary>
    public Guid OwnerId { get; set; }
    public string DisplayName { get; set; }
    public List<Account> AccountInChannels { get; set; }
    public List<Channel> Channels { get; set; }
    public List<User> Users { get; set; }
    public string Description { get; set; }

    public Dictionary<string, string> CommandAlterations { get; set; }
    public Dictionary<string, string> Translations { get; set; }
}
