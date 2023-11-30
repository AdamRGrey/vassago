namespace vassago.Models;

using System;
using System.ComponentModel.DataAnnotations.Schema;

public class ChannelPermissions
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public ulong? MaxAttachmentBytes { get; set; }
    public uint? MaxTextChars { get; set; }
    public bool? LinksAllowed { get; set; }
    public bool? ReactionsPossible { get; set; }
    public Enumerations.LewdnessFilterLevel? LewdnessFilterLevel { get; set; }
    public Enumerations.MeannessFilterLevel? MeannessFilterLevel { get; set; }

    internal DefinitePermissionSettings Definite()
    {
        return new DefinitePermissionSettings()
        {
            MaxAttachmentBytes = this.MaxAttachmentBytes ?? 0,
            MaxTextChars = this.MaxTextChars ?? 0,
            LinksAllowed = this.LinksAllowed ?? false,
            LewdnessFilterLevel = this.LewdnessFilterLevel ?? Enumerations.LewdnessFilterLevel.G,
            MeannessFilterLevel = this.MeannessFilterLevel ?? Enumerations.MeannessFilterLevel.Strict,
            ReactionsPossible = this.ReactionsPossible ?? false
        };
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