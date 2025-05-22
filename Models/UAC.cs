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
    ///<summary>"but past adam", you may ask. "if UACs are configured before runtime, why not write html into your source control, as part of the project,
    ///with the benefit of an html editor?"
    ///absolutely fair question. **But**: the plan is for external services, e.g., over kafka, to manage their own. So from Vassago's perspective,
    ///it's variably before and after compile time. shrug.emote.
    ///</summary>
    public string Description { get; set; }
}
