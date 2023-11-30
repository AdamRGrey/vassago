namespace vassago.Models;

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using static vassago.Models.Enumerations;

public enum WellknownPermissions
{
    Administrator,
    TwitchSummon,
}

public class FeaturePermission
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string InternalName { get; set; }
    public WellknownPermissions? InternalTag { get; set; }

    //a permissions-needing-feature can determine how to use these, but a default "matches" is provided
    //for a message to "match", it must match in every category for which there are candidates.
    //e.g., Administrator is going to be restricted to Users only, and that'll be me
    //e.g., my future Torrent feature would be restricted to accounts and channels.
    //hmmm, what would be inheritable and what wouldn't?
    public IEnumerable<User> RestrictedToUsers { get; set; }
    public IEnumerable<Account> RestrictedToAccounts { get; set; }
    public IEnumerable<Channel> RestrictedToChannels { get; set; }
    public bool Inheritable { get; set; } = true;

    public bool Matches(Message message)
    {
        if(RestrictedToUsers?.Count() > 0)
        {
            if(RestrictedToUsers.FirstOrDefault(u => u.Id == message.Author.IsUser.Id) == null)
            {
                return false;
            }
        }

        if(RestrictedToChannels?.Count() > 0)
        {
            if(Inheritable)
            {
                var found = false;
                var walker = message.Channel;
                if (RestrictedToChannels.FirstOrDefault(c => c.Id == walker.Id) != null)
                {
                    found = true;
                }
                else
                {
                    while (walker.ParentChannel != null)
                    {
                        walker = walker.ParentChannel;
                        if(walker.Users.FirstOrDefault(a => a.ExternalId == message.Author.ExternalId) == null)
                        {
                            //the chain is broken; I don't exist in this channel
                            break;
                        }
                        if (RestrictedToChannels.FirstOrDefault(c => c.Id == walker.Id) != null)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                {

                    if(RestrictedToAccounts?.Count() > 0)
                    {
                        //walker is the "actual" restricted-to channel, but we're inheriting
                        if(walker.Users.FirstOrDefault(a => a.Id == message.Author.Id) == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if(RestrictedToChannels.FirstOrDefault(c => c.Id == message.Channel.Id) == null)
                {
                    return false;
                }
            }
        }
        if(RestrictedToAccounts?.Count() > 0)
        {
            if(RestrictedToAccounts.FirstOrDefault(a => a.Id == message.Author.Id) == null)
            {
                return false;
            }
        }

        //if I got all the way down here, I must be good
        return true;
    }
}
