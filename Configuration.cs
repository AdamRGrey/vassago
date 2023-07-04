using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using vassago.TwitchInterface;

namespace vassago
{
    public class Configuration
    {
        public string ExchangePairsLocation { get; set; }
        public IEnumerable<string> DiscordTokens { get; set; }
        public IEnumerable<TwitchConfig> TwitchConfigs { get; set; }
        public string DBConnectionString { get; set; }
    }
}