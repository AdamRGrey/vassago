namespace vassago.Behavior;

using vassago.Models;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;

public abstract class Behavior
{
    public abstract Task<bool> ActOn(Message message);

    public virtual bool ShouldAct(Message message)
    {
        if(Behaver.Instance.Selves.Any(acc => acc.Id == message.Author.Id))
            return false;
        return Regex.IsMatch(message.Content, $"{Trigger}\\b", RegexOptions.IgnoreCase);
    }

    public abstract string Name { get; }
    public abstract string Trigger { get; }
    public virtual string Description => Name;
}


public class StaticPlzAttribute : Attribute {}