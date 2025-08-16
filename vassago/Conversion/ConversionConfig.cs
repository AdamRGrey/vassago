using System;
using System.Collections.Generic;

namespace vassago.Conversion
{
    public class ConversionConfig
    {
        public class KnownUnit
        {
            public string Canonical { get; set; }
            public IEnumerable<string> Aliases { get; set; }
        }
        public class FormulaicPair
        {
            public string item1 { get; set; }
            public string item2 { get; set; }
            public string formulaforward {get; set; }
            public string formulabackward {get; set; }
        }
        public IEnumerable<KnownUnit> Units { get; set; }
        public IEnumerable<FormulaicPair> FormulaicPairs { get; set; }
    }
}
