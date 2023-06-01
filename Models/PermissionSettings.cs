namespace vassago.Models;

using System;
public class PermissionSettings
{
    public int Id { get; set; }
    public uint? MaxAttachmentBytes { get; set; }
    public uint? MaxTextChars { get; set; }
    public bool? LinksAllowed { get; set; }
    public int? LewdnessFilterLevel { get; set; }
    public int? MeannessFilterLevel { get; set; }
}
