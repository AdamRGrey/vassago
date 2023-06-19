namespace vassago.Models;

using System;
using System.ComponentModel.DataAnnotations.Schema;

public class PermissionSettings
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public uint? MaxAttachmentBytes { get; set; }
    public uint? MaxTextChars { get; set; }
    public bool? LinksAllowed { get; set; }
    public bool? ReactionsPossible { get; set; }
    public int? LewdnessFilterLevel { get; set; }
    public int? MeannessFilterLevel { get; set; }
}
