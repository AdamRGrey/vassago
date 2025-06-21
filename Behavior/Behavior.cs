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
    //recommendation: set up your UACs in your constructor.
    public abstract Task<bool> ActOn(Message message);

    public virtual bool ShouldAct(Message message, List<UAC> matchedUACs)
    {
        if(Behaver.Instance.IsSelf(message.Author.Id))
            return false;
        var triggerTarget = Trigger ;
        foreach(var uacMatch in matchedUACs)
        {
            foreach(var substitution in uacMatch.CommandAlterations)
            {
                triggerTarget = new Regex(substitution.Key).Replace(triggerTarget, substitution.Value);
            }
        }
        return Regex.IsMatch(message.TranslatedContent, $"{triggerTarget}\\b", RegexOptions.IgnoreCase);
    }

    public abstract string Name { get; }
    public abstract string Trigger { get; }
    public virtual string Description => Name;
}

///<summary>
///the behavior should be static. I.e., we make one at the start and it's ready to check and go for the whole lifetime.
///As opposed to LaughAtOwnJoke, which only needs to be created to wait for 1 punchline one time.
///</summary>
public class StaticPlzAttribute : Attribute {}
