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
public class GeneralSnarkPlaying : Behavior
{
    public override string Name => "playin Snarkiness";

    public override string Trigger => "he thinks i'm playin";

    public override string Description => "I didn't think you were playing, but now I do";

    public override bool ShouldAct(Message message)
    {
        if(Behaver.Instance.IsSelf(message.Author.Id))
            return false;

        if((MeannessFilterLevel)message.Channel.EffectivePermissions.MeannessFilterLevel < MeannessFilterLevel.Medium ||
            (LewdnessFilterLevel)message.Channel.EffectivePermissions.LewdnessFilterLevel  < LewdnessFilterLevel.Moderate)
            return false;

        return Regex.IsMatch(message.Content, "^(s?he|(yo)?u|y'?all|they) thinks? i'?m (playin|jokin|kiddin)g?$", RegexOptions.IgnoreCase);
    }
    public override async Task<bool> ActOn(Message message)
    {
        await message.Channel.SendMessage("I believed you for a second, but then you assured me you's a \uD83C\uDDE7   \uD83C\uDDEE   \uD83C\uDDF9   \uD83C\uDDE8   \uD83C\uDDED");
        return true;
    }
}