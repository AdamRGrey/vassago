namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using static vassago.Models.Enumerations;

public class Channel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string ExternalId { get; set; }
    public string DisplayName { get; set; }
    public List<Channel> SubChannels { get; set; }
    public Channel ParentChannel { get; set; }
    public string Protocol { get; set; }
    public List<Message> Messages { get; set; }
    public List<Account> Users { get; set; }
    public ChannelType ChannelType {get; set; }

    //Permissions
    public ulong? MaxAttachmentBytes { get; set; }
    public uint? MaxTextChars { get; set; }
    public bool? LinksAllowed { get; set; }
    public bool? ReactionsPossible { get; set; }
    public Enumerations.LewdnessFilterLevel? LewdnessFilterLevel { get; set; }
    public Enumerations.MeannessFilterLevel? MeannessFilterLevel { get; set; }

    [NonSerialized]
    public Func<string, string, Task> SendFile;

    [NonSerialized]
    public Func<string, Task> SendMessage;


    public DefinitePermissionSettings EffectivePermissions
    {
        get
        {
            var path = new Stack<Channel>(); //omg i actually get to use a data structure from university
            var walker = this;
            path.Push(this);
            while(walker.ParentChannel != null)
            {
                walker = walker.ParentChannel;
                path.Push(walker);
            }
            DefinitePermissionSettings toReturn = new DefinitePermissionSettings();
            walker = path.Pop();
            while(walker != null)
            {
                toReturn.LewdnessFilterLevel = LewdnessFilterLevel ?? toReturn.LewdnessFilterLevel;
                toReturn.MeannessFilterLevel = MeannessFilterLevel ?? toReturn.MeannessFilterLevel;
                toReturn.LinksAllowed = LinksAllowed ?? toReturn.LinksAllowed;
                toReturn.MaxAttachmentBytes = MaxAttachmentBytes ?? toReturn.MaxAttachmentBytes;
                toReturn.MaxTextChars = MaxTextChars ?? toReturn.MaxTextChars;
                toReturn.ReactionsPossible = ReactionsPossible ?? toReturn.ReactionsPossible;

                walker = path.Pop();
            }

            return toReturn;
        }
    }
    public string LineageSummary
    {
        get
        {
            if(this.ParentChannel != null)
            {
                return this.ParentChannel.LineageSummary + '/' + this.DisplayName;
            }
            else
            {
                return this.Protocol;
            }
        }
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