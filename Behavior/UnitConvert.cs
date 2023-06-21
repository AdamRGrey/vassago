namespace vassago.Behavior;

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class UnitConvert : Behavior
{
    public override string Name => "Unit conversion";

    public override string Trigger => "!freedomunits";
    public override string Description => "convert between many units.";

    public override async Task<bool> ActOn(Message message)
    {

        var theseMatches = Regex.Matches(message.Content, "\\b([\\d]+\\.?\\d*) ?([^\\d\\s].*) (in|to|as) ([^\\d\\s].*)$", RegexOptions.IgnoreCase);

        if (theseMatches != null && theseMatches.Count > 0 && theseMatches[0].Groups != null && theseMatches[0].Groups.Count == 5)
        {
            decimal asNumeric = 0;
            if (decimal.TryParse(theseMatches[0].Groups[1].Value, out asNumeric))
            {
                await message.Channel.SendMessage(Conversion.Converter.Convert(asNumeric, theseMatches[0].Groups[2].Value, theseMatches[0].Groups[4].Value.ToLower()));
                return true;
            }
            await message.Channel.SendMessage("mysteriously semi-parsable");
            return true;
        }
        await message.Channel.SendMessage( "unparsable");
        return true;
    }
}