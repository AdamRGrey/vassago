namespace vassago.Models;

using System;
using System.ComponentModel.DataAnnotations.Schema;

public class Attachment
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public ulong? ExternalId { get; set; }
    public Uri Source { get; set; }
    public byte[] Content { get; set; }
    public string Filename { get; set; }
    public Message Message { get; set; }
    public string ContentType { get; internal set; }
    public string Description { get; internal set; }
    public int Size { get; internal set; }
}