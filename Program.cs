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
using vassago.Models;

namespace vassago
{
    class Program
    {
        Configuration config = Configuration.Parse("appsettings.json");
        private List<DiscordInterface.DiscordInterface> discords = new List<DiscordInterface.DiscordInterface>();

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();
        public async Task MainAsync()
        {
            Shared.DBConnectionString = config.DBConnectionString;
            Shared.dbContext = new ChattingContext();
            {   
                Shared.dbContext.Database.EnsureCreated();
            }
            Conversion.Converter.Load(config.ExchangePairsLocation);
            if(config.DiscordTokens.Any())
                foreach(var dt in config.DiscordTokens)
                {
                    var d = new DiscordInterface.DiscordInterface();
                    await d.Init(dt);
                    discords.Add(d);
                }            

            await Task.Delay(-1);
        }
    }
}