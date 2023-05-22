using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Diagnostics;
using vassago.Discord_Vassago;

namespace vassago
{
    class Program
    {
        Configuration config = Configuration.Parse("appsettings.json");
        private List<DiscordInterface> discords = new List<DiscordInterface>();

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();
        public async Task MainAsync()
        {
            Conversion.Converter.Load(config.ExchangePairsLocation);
            if(config.DiscordTokens.Any())
                foreach(var dt in config.DiscordTokens)
                {
                    var d = new DiscordInterface();
                    await d.Init(dt);
                    discords.Add(d);
                }            

            await Task.Delay(-1);
        }
    }
}