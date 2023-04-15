using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using QRCoder;

namespace silverworker_discord.Conversion
{
    public static class Converter
    {

        private delegate decimal Convert1Way(decimal input);
        private static string currencyPath;
        private static ExchangePairs currencyConf = null;
        private static DateTime lastUpdatedCurrency = DateTime.UnixEpoch;
        private static List<Tuple<string, string, Convert1Way, Convert1Way>> knownConversions = new List<Tuple<string, string, Convert1Way, Convert1Way>>()
        {
            new Tuple<string, string, Convert1Way, Convert1Way>("℉", "°C", (f => {return(f- 32.0m) / 1.8m;}), (c => {return  1.8m*c + 32.0m;})),
        };
        private static Dictionary<List<string>, string> knownAliases = new Dictionary<List<string>, string>(new List<KeyValuePair<List<string>, string>>());

        public static string convert(string message)
        {
            var theseMatches = Regex.Matches(message, "\\b([\\d]+\\.?\\d*) ?([^\\d\\s].*) (in|to|as) ([^\\d\\s].*)$", RegexOptions.IgnoreCase);

            if (theseMatches != null && theseMatches.Count > 0 && theseMatches[0].Groups != null && theseMatches[0].Groups.Count == 5)
            {
                decimal asNumeric = 0;
                if (decimal.TryParse(theseMatches[0].Groups[1].Value, out asNumeric))
                {
                    return actualConvert(asNumeric, theseMatches[0].Groups[2].Value, theseMatches[0].Groups[4].Value.ToLower());
                }
                return "mysteriously semi-parsable";
            }
            return "unparsable";
        }

        public static void Load(string currencyPath)
        {
            Converter.currencyPath = currencyPath;
            var convConf = JsonConvert.DeserializeObject<ConversionConfig>(File.ReadAllText("assets/conversion.json"));
            foreach (var unit in convConf.Units)
            {
                knownAliases.Add(unit.Aliases.ToList(), unit.Canonical);
            }
            foreach (var lp in convConf.LinearPairs)
            {
                AddLinearPair(lp.item1, lp.item2, lp.factor);
            }
            Task.Run(async () => {
                while(true)
                {
                    loadCurrency();
                    await Task.Delay(TimeSpan.FromHours(8));
                }
            });
        }
        private static void loadCurrency()
        {
            Console.WriteLine("loading currency exchange data.");
            if(currencyConf != null)
            {
                knownConversions.RemoveAll(kc => kc.Item1 == currencyConf.Base);
                knownAliases.Remove(knownAliases.FirstOrDefault(kvp => kvp.Value == currencyConf.Base).Key);
                foreach (var rate in currencyConf.rates)
                    knownAliases.Remove(knownAliases.FirstOrDefault(kvp => kvp.Value == rate.Key).Key);
            }
            if (File.Exists(currencyPath))
            {
                currencyConf = JsonConvert.DeserializeObject<ExchangePairs>(File.ReadAllText(currencyPath));

                knownAliases.Add(new List<string>() { currencyConf.Base.ToLower() }, currencyConf.Base);
                foreach (var rate in currencyConf.rates)
                {
                    knownAliases.Add(new List<string>() { rate.Key.ToLower() }, rate.Key);
                    AddLinearPair(currencyConf.Base, rate.Key, rate.Value);
                }
            }
        }

        private static string actualConvert(decimal numericTerm, string sourceunit, string destinationUnit)
        {
            var normalizedSourceUnit = normalizeUnit(sourceunit);
            if (string.IsNullOrWhiteSpace(normalizedSourceUnit))
            {
                return $"what's {sourceunit}?";
            }
            var normalizedDestUnit = normalizeUnit(destinationUnit);
            if (string.IsNullOrWhiteSpace(normalizedDestUnit))
            {
                return $"what's {destinationUnit}?";
            }
            if (normalizedSourceUnit == normalizedDestUnit)
            {
                return $"source and dest are the same, so... {numericTerm} {normalizedDestUnit}?";
            }
            var foundPath = exhaustiveBreadthFirst(normalizedDestUnit, new List<string>() { normalizedSourceUnit })?.ToList();

            if (foundPath != null)
            {
                var accumulator = numericTerm;
                for (int j = 0; j < foundPath.Count - 1; j++)
                {
                    var forwardConversion = knownConversions.FirstOrDefault(kc => kc.Item1 == foundPath[j] && kc.Item2 == foundPath[j + 1]);
                    if (forwardConversion != null)
                    {
                        accumulator = forwardConversion.Item3(accumulator);
                    }
                    else
                    {
                        var reverseConversion = knownConversions.First(kc => kc.Item2 == foundPath[j] && kc.Item1 == foundPath[j + 1]);
                        accumulator = reverseConversion.Item4(accumulator);
                    }
                }
                if (normalizedDestUnit == currencyConf.Base || currencyConf.rates.Select(r => r.Key).Contains(normalizedDestUnit))
                {
                    return $"{String.Format("approximately {0:0.00}", accumulator)} {normalizedDestUnit} as of {currencyConf.DateUpdated.ToLongDateString()}";
                }
                else
                {
                    return $"{String.Format("{0:G4}", accumulator)} {normalizedDestUnit}";
                }
            }
            return "no conversion known";
        }
        private static string normalizeUnit(string unit)
        {
            var normalizedUnit = unit.ToLower();
            if (normalizedUnit.EndsWith("es"))
            {
                normalizedUnit = normalizedUnit.Substring(0, normalizedUnit.Length - 2);
            }
            else if (normalizedUnit.EndsWith('s'))
            {
                normalizedUnit = normalizedUnit.Substring(0, normalizedUnit.Length - 1);
            }
            if (knownConversions.FirstOrDefault(c => c.Item1 == normalizedUnit || c.Item2 == normalizedUnit) != null)
            {
                return normalizedUnit;
            }
            if (!knownAliases.ContainsValue(normalizedUnit))
            {
                var key = knownAliases.Keys.FirstOrDefault(listkey => listkey.Contains(normalizedUnit));
                if (key != null)
                {
                    return knownAliases[key];
                }
            }
            return null;
        }
        private static IEnumerable<string> exhaustiveBreadthFirst(string dest, IEnumerable<string> currentPath)
        {
            var last = currentPath.Last();
            if (last == dest)
            {
                return currentPath;
            }

            var toTest = new List<List<string>>();
            foreach (var conv in knownConversions)
            {
                if (conv.Item1 == last && currentPath.Contains(conv.Item2) == false && conv.Item3 != null)
                {
                    var test = exhaustiveBreadthFirst(dest, currentPath.Append(conv.Item2));
                    if (test != null)
                        return test;
                }
                if (conv.Item2 == last && currentPath.Contains(conv.Item1) == false && conv.Item4 != null)
                {
                    var test = exhaustiveBreadthFirst(dest, currentPath.Append(conv.Item1));
                    if (test != null)
                        return test;
                }
            }
            return null;
        }
        private static void AddLinearPair(string key1, string key2, decimal factor)
        {
            var reverseFactor = 1.0m / factor;
            knownConversions.Add(new Tuple<string, string, Convert1Way, Convert1Way>(
                key1, key2, x => x * factor, y => y * reverseFactor
            ));
        }
    }
}