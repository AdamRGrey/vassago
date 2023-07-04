namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public List<Account> Accounts { get; set; }
    //permissions are per account-in-channel or per-user, and always propagate down. and since protocol will be a channel, I'll set the "is adam" permission on myself 1x/protocol.
    public List<Enumerations.WellknownPermissions> PermissionTags{get;set;}
}