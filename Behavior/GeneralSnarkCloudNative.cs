namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using vassago.Models;
using static vassago.Models.Enumerations;

[StaticPlz]
public class GeneralSnarkCloudNative : Behavior
{
    public override string Name => "general snarkiness: cloud native";

    public override string Trigger => "certain tech buzzwords that no human uses in normal conversation";
    public override bool ShouldAct(Message message)
    {
        if(Behaver.Instance.IsSelf(message.Author.Id))
            return false;

        if(message.Channel.EffectivePermissions.ReactionsPossible)
            return false;

        if((MeannessFilterLevel)message.Channel.EffectivePermissions.MeannessFilterLevel < MeannessFilterLevel.Medium)
            return false;

        return Regex.IsMatch(message.Content, "\\bcloud( |-)?native\\b", RegexOptions.IgnoreCase) ||
               Regex.IsMatch(message.Content, "\\benterprise( |-)?(level|solution)\\b", RegexOptions.IgnoreCase);
    }

    public override async Task<bool> ActOn(Message message)
    {
        switch (Shared.r.Next(2))
        {
            case 0:
                await message.React("\uD83E\uDD2E"); //vomit emoji
                break;
            case 1:
                await message.React("\uD83C\uDDE7"); //B emoji
                await message.React("\uD83C\uDDE6"); //A
                await message.React("\uD83C\uDDF3"); //N
                break;
        }
        return true;
    }
}