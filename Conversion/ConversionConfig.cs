using System;
using System.Collections.Generic;

namespace silverworker_discord.Conversion
{
    public class ConversionConfig
    {
        public class KnownUnit
        {
            public string Canonical { get; set; }
            public IEnumerable<string> Aliases { get; set; }
        }
        public class LinearPair
        {
            public string item1 { get; set; }
            public string item2 { get; set; }
            public decimal factor { get; set; }
        }
        public IEnumerable<KnownUnit> Units { get; set; }
        public IEnumerable<LinearPair> LinearPairs { get; set; }
    }
}