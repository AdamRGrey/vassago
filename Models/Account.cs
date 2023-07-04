namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;

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
    //permissions are per account-in-channel or per-user, and always propagate down. and since protocol will be a channel, I'll set the "is adam" permission on myself 1x/protocol.
    public List<Enumerations.WellknownPermissions> PermissionTags{get;set;}
    public string Protocol { get; set; }
    public User IsUser {get; set;}
}