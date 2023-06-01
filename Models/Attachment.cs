namespace vassago.Models;

using System;
public class Attachment
{
    public Guid Id { get; set; }
    public ulong? ExternalId { get; set; }
    public Uri Source { get; set; }
    public byte[] Content { get; set; }
    public string Filename { get; set; }
    public Message Message { get; set; }
}