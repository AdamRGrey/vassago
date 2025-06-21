namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using vassago.Models;


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
    public Guid OwnerId { get; set;}
    public string DisplayName { get; set; }
    public List<Account> AccountInChannels { get; set; }
    public List<Channel> Channels { get; set; }
    public List<User> Users { get; set; }
    ///<summary>"but past adam", you may ask. "if UACs are configured before runtime, why not write html into your source control, as part of the project,
    ///with the benefit of an html editor?"
    ///absolutely fair question. **But**: the plan is for external services, e.g., over kafka, to manage their own. So from Vassago's perspective,
    ///it's variably before and after compile time. shrug.emote.
    ///</summary>
    public string Description { get; set; }

    public Dictionary<string, string> CommandAlterations {get; set;}
    public Dictionary<string, string> Translations {get; set;}
}
