using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace vassago
{
    public class Configuration
    {
        public string ExchangePairsLocation { get; set; }
        public IEnumerable<string> DiscordTokens { get; set; }
        public IEnumerable<Tuple<string, string>> TwitchTokens { get; set; }
        public string DBConnectionString { get; set; }
    }
}