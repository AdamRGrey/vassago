using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace vassago
{
    public class Configuration
    {
        public string ExchangePairsLocation {get;set;}
        public IEnumerable<string> DiscordTokens { get; set; }
        public string DBConnectionString{get;set;}

        private Configuration(){}
        public static Configuration Parse(string configurationPath)
        {
            if(string.IsNullOrWhiteSpace(configurationPath))
                return null;

            if (!File.Exists(configurationPath))
            {
                File.WriteAllText("sample-appsettings.json", JsonConvert.SerializeObject(new Configuration(), Formatting.Indented));
                throw new ConfigurationException($"could not find configuration at {configurationPath}! copying sample to that spot.");
            }
            var fileContents = File.ReadAllText(configurationPath);
            if (string.IsNullOrWhiteSpace(fileContents))
            {
                File.WriteAllText("sample-appsettings.json", JsonConvert.SerializeObject(new Configuration(), Formatting.Indented));
                throw new ConfigurationException($"configuration file at {configurationPath} was empty! overwriting with sample settings.");
            }

            var conf = JsonConvert.DeserializeObject<Configuration>(fileContents);

            if (conf == null)
            {
                File.WriteAllText("sample-appsettings.json", JsonConvert.SerializeObject(new Configuration(), Formatting.Indented));
                throw new ConfigurationException($"configuration file at {configurationPath} was empty! overwriting with sample settings.");
            }
            return conf;
        }
        public class ConfigurationException : Exception
        {
            public ConfigurationException(string message) : base(message){}
        }
    }
}