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
using Jace;
using Newtonsoft.Json;
using QRCoder;

namespace vassago.Conversion
{
    public static class Converter
    {
        private static string currencyPath;
        private static ExchangePairs currencyConf = null;
        private static DateTime lastUpdatedCurrency = DateTime.UnixEpoch;
        private static List<Tuple<string, string,Func<double, double>, Func<double, double>>> knownConversions = new List<Tuple<string, string, Func<double, double>,Func<double, double>>>();
        private static Dictionary<List<string>, string> knownAliases = new Dictionary<List<string>, string>(new List<KeyValuePair<List<string>, string>>());
        private static CalculationEngine engine = new CalculationEngine();
        public static string DebugInfo()
        {
            var convertibles = knownConversions.Select(kc => kc.Item1).Union(knownConversions.Select(kc => kc.Item2)).Union(
                knownAliases.Keys.SelectMany(k => k)).Distinct();
            return $"{convertibles.Count()} convertibles; {string.Join(", ", convertibles)}";
        }

        private static async Task currencyLoadLoop()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromHours(8));
                loadCurrency();
            }
        }
        public static void Load(string currencyPath)
        {
            Converter.currencyPath = currencyPath;
            loadStatic();
            Task.Run(currencyLoadLoop);
        }
        private static void loadStatic()
        {
            knownConversions = new List<Tuple<string, string, Func<double, double>, Func<double, double>>>();
            knownAliases = new Dictionary<List<string>, string>(new List<KeyValuePair<List<string>, string>>());
            var convConf = JsonConvert.DeserializeObject<ConversionConfig>(File.ReadAllText("assets/conversion.json").ToLower());
            foreach (var unit in convConf.Units)
            {
                knownAliases.Add(unit.Aliases.ToList(), unit.Canonical);
            }
            foreach (var lp in convConf.FormulaicPairs)
            {
                AddLinearPair(lp.item1, lp.item2, lp.formulaforward, lp.formulabackward );
            }
            loadCurrency();
        }
        private static void loadCurrency()
        {
            Console.WriteLine("loading currency exchange data.");
            if (currencyConf != null)
            {
                knownConversions.RemoveAll(kc => kc.Item1 == currencyConf.Base);
            }
            if (File.Exists(currencyPath))
            {
                currencyConf = JsonConvert.DeserializeObject<ExchangePairs>(File.ReadAllText(currencyPath).ToLower());

                if (!knownAliases.ContainsValue(currencyConf.Base))
                {
                    knownAliases.Add(new List<string>() { }, currencyConf.Base);
                }
                foreach (var rate in currencyConf.rates)
                {
                    if (!knownAliases.ContainsValue(rate.Key))
                    {
                        knownAliases.Add(new List<string>() { rate.Key.ToLower() }, rate.Key);
                    }
                    AddLinearPair(currencyConf.Base, rate.Key, $"i1 * {rate.Value}", $"i1 / {rate.Value}");
                }
            }
        }

        public static string Convert(double numericTerm, string sourceunit, string destinationUnit)
        {
            //normalize units
            var normalizationAttemptSource = NormalizeUnit(sourceunit.ToLower());
            if (normalizationAttemptSource?.Count() == 0)
            {
                return $"can't find {sourceunit}";
            }
            var normalizedSourceUnit = normalizationAttemptSource.First();

            var normalizationAttemptDest = NormalizeUnit(destinationUnit.ToLower());
            if (normalizationAttemptDest?.Count() == 0)
            {
                return $"can't find {destinationUnit}";
            }
            var normalizedDestUnit = normalizationAttemptDest.First();
            if (normalizedSourceUnit == normalizedDestUnit)
            {
                return $"source and dest are the same, so... {numericTerm} {normalizedDestUnit}?";
            }
            var foundPath = exhaustiveBreadthFirst(normalizedDestUnit, new List<string>() { normalizedSourceUnit })?.ToList();

            //resolve ambiguity
            var disambiguationPaths = new List<List<string>>();
            if (normalizationAttemptSource.Count() > 1 && normalizationAttemptDest.Count() > 1)
            {
                foreach (var possibleSourceUnit in normalizationAttemptSource)
                {
                    foreach (var possibleDestUnit in normalizationAttemptDest)
                    {
                        foundPath = exhaustiveBreadthFirst(possibleDestUnit, new List<string>() { possibleSourceUnit })?.ToList();
                        if (foundPath != null)
                        {
                            disambiguationPaths.Add(foundPath.ToList());
                            normalizedSourceUnit = possibleSourceUnit;
                            normalizedDestUnit = possibleDestUnit;
                        }
                    }
                }
            }
            else if (normalizationAttemptSource.Count() > 1)
            {
                foreach (var possibleSourceUnit in normalizationAttemptSource)
                {
                    foundPath = exhaustiveBreadthFirst(normalizedDestUnit, new List<string>() { possibleSourceUnit })?.ToList();
                    if (foundPath != null)
                    {
                        disambiguationPaths.Add(foundPath.ToList());
                        normalizedSourceUnit = possibleSourceUnit;
                    }
                }
            }
            else if (normalizationAttemptDest.Count() > 1)
            {
                foreach (var possibleDestUnit in normalizationAttemptDest)
                {
                    foundPath = exhaustiveBreadthFirst(possibleDestUnit, new List<string>() { normalizedSourceUnit })?.ToList();
                    if (foundPath != null)
                    {
                        disambiguationPaths.Add(foundPath.ToList());
                        normalizedDestUnit = possibleDestUnit;
                    }
                }
            }
            if (disambiguationPaths.Count() > 1)
            {
                var sb = new StringBuilder();
                sb.Append("unresolvable ambiguity.");
                foreach(var possibility in disambiguationPaths)
                {
                    sb.Append($" {possibility.First()} -> {possibility.Last()}?");
                }
                return sb.ToString();
            }

            if (disambiguationPaths.Count() == 1)
            {
                //TODO: I'm not entirely sure this is necessary.
                foundPath = disambiguationPaths.First();
            }
            //actually do the math.
            if (foundPath != null)
            {
                var accumulator = numericTerm;
                for (int j = 0; j < foundPath.Count() - 1; j++)
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
                if (currencyConf != null && (
                        (normalizedDestUnit == currencyConf.Base || currencyConf.rates.Select(r => r.Key).Contains(normalizedDestUnit))
                        && (normalizedSourceUnit == currencyConf.Base || currencyConf.rates.Select(r => r.Key).Contains(normalizedSourceUnit))
                    ))
                {
                    return $"{String.Format("approximately {0:0.00}", accumulator)} {normalizedDestUnit} as of {currencyConf.DateUpdated.ToLongDateString()}";
                }
                else
                {
                    if (String.Format("{0:G3}", accumulator).Contains("E-"))
                    {
                        return $"{accumulator} {normalizedDestUnit}";
                    }
                    else
                    {
                        return $"{String.Format("{0:N}", accumulator)} {normalizedDestUnit}";
                    }
                }
            }
            return "dimensional analysis failure - I know those units but can't find a path between them.";
        }
        private static List<string> NormalizeUnit(string unit)
        {
            if (string.IsNullOrWhiteSpace(unit))
                return new();
            var normalizedUnit = unit;
            //first, if it does exist in conversions, that's the canonical name.
            if (knownConversions.FirstOrDefault(c => c.Item1 == normalizedUnit || c.Item2 == normalizedUnit) != null)
            {
                return new List<string>() { normalizedUnit };
            }
            //if "unit" isn't a canonical name... actually it never should be; a conversion should use it.
            if (!knownAliases.ContainsValue(normalizedUnit))
            {
                //then we look through aliases...
                var keys = knownAliases.Keys.Where(listkey => listkey.Contains(normalizedUnit));
                if (keys?.Count() > 1)
                {
                    var toReturn = new List<string>();
                    foreach (var key in keys)
                    {
                        toReturn.Add(knownAliases[key]);
                    }
                    return toReturn;
                }
                else if (keys.Count() == 1)
                {
                    //for the canonical name.
                    return new List<string>() { knownAliases[keys.First()] };
                }
            }
            if (normalizedUnit.EndsWith("es"))
            {
                return NormalizeUnit(normalizedUnit.Substring(0, normalizedUnit.Length - 2));
            }
            else if (normalizedUnit.EndsWith('s'))
            {
                return NormalizeUnit(normalizedUnit.Substring(0, normalizedUnit.Length - 1));
            }
            return new();
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
        private static void AddLinearPair(string key1, string key2, string formulaForward, string formulaBackward)
        {
            knownConversions.Add(new Tuple<string, string, Func<double, double>, Func<double, double>>(
                                     key1,
                                     key2,
                                     (Func<double, double>) engine.Formula(formulaForward)
                                         .Parameter("i1", DataType.FloatingPoint)
                                     .Result(DataType.FloatingPoint)
                                     .Build(),
                                     (Func<double, double>) engine.Formula(formulaBackward)
                                         .Parameter("i1", DataType.FloatingPoint)
                                     .Result(DataType.FloatingPoint)
                                     .Build()
                                 ));
        }
    }
}
