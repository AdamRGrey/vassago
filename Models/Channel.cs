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
    public string ExternalId { get; set; }
    public string DisplayName { get; set; }
    public bool IsDM { get; set; }
    public PermissionSettings Permissions { get; set; }
    public List<Channel> SubChannels { get; set; }
    public Channel ParentChannel { get; set; }
    public string Protocol { get; set; }
    public List<Message> Messages { get; set; }
    public List<Account> Users { get; set; }
    //public Dictionary<string, string> EmoteOverrides{get;set;}

    [NonSerialized]
    public Func<string, string, Task> SendFile;

    [NonSerialized]
    public Func<string, Task> SendMessage;


    public PermissionSettings EffectivePermissions
    {
        get
        {
            PermissionSettings toReturn = Permissions ?? new PermissionSettings();
            return GetEffectivePermissions(ref toReturn);
        }
    }
    private PermissionSettings GetEffectivePermissions(ref PermissionSettings settings)
    {
        if(settings == null) throw new ArgumentNullException();
        settings.LewdnessFilterLevel = settings.LewdnessFilterLevel ?? Permissions?.LewdnessFilterLevel;
        settings.MeannessFilterLevel = settings.MeannessFilterLevel ?? Permissions?.MeannessFilterLevel;
        settings.LinksAllowed = settings.LinksAllowed ?? Permissions?.LinksAllowed;
        settings.MaxAttachmentBytes = settings.MaxAttachmentBytes ?? Permissions?.MaxAttachmentBytes;
        settings.MaxTextChars = settings.MaxTextChars ?? Permissions?.MaxTextChars;
        settings.ReactionsPossible = settings.ReactionsPossible ?? Permissions?.ReactionsPossible;

        if(this.ParentChannel != null &&
            (settings.LewdnessFilterLevel == null ||
            settings.MeannessFilterLevel == null ||
            settings.LinksAllowed  == null ||
            settings.MaxAttachmentBytes == null ||
            settings.MaxTextChars == null ||
            settings.ReactionsPossible == null))
        {
            return this.ParentChannel.GetEffectivePermissions(ref settings);
        }
        else
        {
            return settings;
        }
    }
}