namespace vassago.Behavior;

using vassago.Models;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;

//expect a behavior to be created per mesage
public abstract class Behavior
{
    //TODO: message should have a channel, which should provide permissions. shouldn't have to pass it here.
    public abstract Task<bool> ActOn(PermissionSettings permissions, Message message);

    public virtual bool ShouldAct(PermissionSettings permissions, Message message)
    {
        return Regex.IsMatch(message.Content, $"{Trigger}\\b", RegexOptions.IgnoreCase);
    }

    public abstract string Name { get; }
    public abstract string Trigger { get; }
    public virtual string Description => Name;
}
