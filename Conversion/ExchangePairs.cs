using System;
using System.Collections.Generic;

namespace silverworker_discord.Conversion
{
    public class ExchangePairs
    {
        public string disclaimer{ get; set; }
        public string license{ get; set; }
        public int timestamp{ get; set; }
        public DateTime DateUpdated { get { return DateTime.UnixEpoch.AddSeconds(timestamp); }}
        public string Base{ get; set; }
        public Dictionary<string, decimal> rates { get; set; }
    }
}