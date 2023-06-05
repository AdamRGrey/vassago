namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

public class User //more like "user's account - no concept of the person outside of the protocol. (yet?)
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public ulong? ExternalId { get; set; }
    public string Username { get; set; } //TODO: display names. many protocols support this feature.
    public bool IsBot { get; set; } //webhook counts
    public Channel SeenInChannel { get; set; }
    public string Protocol { get; set; }
    
    public User(){}
    public User(User u)
    {
        Type t = typeof(User);
        PropertyInfo[] properties = t.GetProperties();
        foreach (PropertyInfo pi in properties)
        {
            pi.SetValue(this, pi.GetValue(u, null), null);
        }
    }
}