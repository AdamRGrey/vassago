namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;
using static vassago.Models.Enumerations;

[StaticPlz]
public class GeneralSnarkMisspellDefinitely : Behavior
{
    public override string Name => "Snarkiness: misspell definitely";

    public override string Trigger => "definitely but not";

    public override string Description => "https://xkcd.com/2871/";

    private Dictionary<string, string> snarkmap = new Dictionary<string, string>()
    {
      {"definetly", "*almost* definitely"},
      {"definately", "probably"},
      {"definatly", "probably not"},
      {"defenitely", "not telling (it's a surprise)"},
      {"defintely", "per the propheecy"},
      {"definetely", "definitely, maybe"},
      {"definantly", "to be decided by coin toss"},
      {"defanitely", "in one universe out of 14 million"},
      {"defineatly", "only the gods know"},
      {"definitly", "unless someone cute shows up"}
    };
    public override bool ShouldAct(Message message, List<UAC> matchedUACs)
    {
        if(Behaver.Instance.IsSelf(message.Author.Id))
            return false;

        // if((MeannessFilterLevel)message.Channel.EffectivePermissions.MeannessFilterLevel < MeannessFilterLevel.Medium)
        //     return false;

        foreach(var k in snarkmap.Keys)
        {
            if( Regex.IsMatch(message.TranslatedContent?.ToLower(), "\\b"+k+"\\b", RegexOptions.IgnoreCase))
                return true;
        }
        return false;
    }
    public override async Task<bool> ActOn(Message message)
    {
        foreach(var k in snarkmap.Keys)
        {
            if( Regex.IsMatch(message.TranslatedContent, "\\b"+k+"\\b", RegexOptions.IgnoreCase))
            {
                Behaver.Instance.Reply(message.Id, k + "? so... " + snarkmap[k] + "?");
                return true;
            }
        }
        return true;
    }
}
