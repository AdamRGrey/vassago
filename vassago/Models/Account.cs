namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json.Serialization;

public class Account
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string ExternalId { get; set; }
    public string Username { get; set; }
    private string _displayName = null;
    public string DisplayName
    {
        get
        {
            return _displayName ?? Username;
        }
        set
        {
            _displayName = value;
        }
    }
    public bool IsBot { get; set; } //webhook counts
    public Channel SeenInChannel { get; set; }
    public string Protocol { get; set; }
    public List<UAC> UACs { get; set; }
    [JsonIgnore]
    public User IsUser {get; set;}
}
