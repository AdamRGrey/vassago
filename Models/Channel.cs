namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static vassago.Models.Enumerations;

public class Channel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string ExternalId { get; set; }
    public string DisplayName { get; set; }
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public List<Channel> SubChannels { get; set; }
    [JsonIgnore]
    public Channel ParentChannel { get; set; }
    public Guid? ParentChannelId { get; set; }
    public string Protocol { get; set; }
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public List<Message> Messages { get; set; }
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public List<Account> Users { get; set; }
    public ChannelType ChannelType { get; set; }

    //Permissions
    public ulong? MaxAttachmentBytes { get; set; }
    public uint? MaxTextChars { get; set; }
    public bool? LinksAllowed { get; set; }
    public bool? ReactionsPossible { get; set; }
    public Enumerations.LewdnessFilterLevel? LewdnessFilterLevel { get; set; }
    public Enumerations.MeannessFilterLevel? MeannessFilterLevel { get; set; }
    public List<UAC> UACs { get; set; }
    //both incoming and outgoing
    //public Dictionary<string, string> Aliases { get; set; }

    public DefinitePermissionSettings EffectivePermissions
    {
        get
        {
            var path = new Stack<Channel>(); //omg i actually get to use a data structure from university
            var walker = this;
            path.Push(this);
            while (walker.ParentChannel != null)
            {
                walker = walker.ParentChannel;
                path.Push(walker);
            }
            DefinitePermissionSettings toReturn = new DefinitePermissionSettings();

            while (path.Count > 0)
            {
                walker = path.Pop();
                toReturn.LewdnessFilterLevel = walker.LewdnessFilterLevel ?? toReturn.LewdnessFilterLevel;
                toReturn.MeannessFilterLevel = walker.MeannessFilterLevel ?? toReturn.MeannessFilterLevel;
                toReturn.LinksAllowed = walker.LinksAllowed ?? toReturn.LinksAllowed;
                toReturn.MaxAttachmentBytes = walker.MaxAttachmentBytes ?? toReturn.MaxAttachmentBytes;
                toReturn.MaxTextChars = walker.MaxTextChars ?? toReturn.MaxTextChars;
                toReturn.ReactionsPossible = walker.ReactionsPossible ?? toReturn.ReactionsPossible;
            }

            return toReturn;
        }
    }
    public string LineageSummary
    {
        get
        {
            if (this.ParentChannel != null)
            {
                return this.ParentChannel.LineageSummary + '/' + this.DisplayName;
            }
            else
            {
                return this.Protocol;
            }
        }
    }

    ///<summary>
    ///break self-referencing loops for library-agnostic serialization
    ///</summary>
    public Channel AsSerializable()
    {
        var toReturn = this.MemberwiseClone() as Channel;
        toReturn.ParentChannel = null;
        if (toReturn.Users?.Count > 0)
        {
            foreach (var account in toReturn.Users)
            {
                account.SeenInChannel = null;
            }
        }
        return toReturn;
    }
}

public class DefinitePermissionSettings
{
    public ulong MaxAttachmentBytes { get; set; }
    public uint MaxTextChars { get; set; }
    public bool LinksAllowed { get; set; }
    public bool ReactionsPossible { get; set; }
    public Enumerations.LewdnessFilterLevel LewdnessFilterLevel { get; set; }
    public Enumerations.MeannessFilterLevel MeannessFilterLevel { get; set; }
}
