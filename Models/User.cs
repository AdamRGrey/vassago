namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

public class User //TODO: distinguish the user and their account
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public ulong? ExternalId { get; set; }
    public string Username { get; set; } //TODO: display names. many protocols support this feature.
    public bool IsBot { get; set; } //webhook counts
    public Channel SeenInChannel { get; set; }
    public string Protocol { get; set; }
}