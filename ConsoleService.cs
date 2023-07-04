namespace vassago
{
    using vassago;
    using vassago.Models;
    using vassago.TwitchInterface;

    internal class ConsoleService : IHostedService
    {
        Configuration config = new Configuration();

        public ConsoleService(IConfiguration aspConfig)
        {
            config.DBConnectionString = aspConfig["DBConnectionString"];
            config.ExchangePairsLocation = aspConfig["ExchangePairsLocation"];
            config.DiscordTokens = aspConfig.GetSection("DiscordTokens").Get<IEnumerable<string>>();
            config.TwitchConfigs = aspConfig.GetSection("TwitchConfigs").Get<IEnumerable<TwitchConfig>>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Shared.DBConnectionString = config.DBConnectionString;
            var dbc = new ChattingContext();
            dbc.Database.EnsureCreated();

            Conversion.Converter.Load(config.ExchangePairsLocation);

            if (config.DiscordTokens?.Any() ?? false)
                foreach (var dt in config.DiscordTokens)
                {
                    var d = new DiscordInterface.DiscordInterface();
                    await d.Init(dt);
                    ProtocolInterfaces.ProtocolList.discords.Add(d);
                }

            if (config.TwitchConfigs?.Any() ?? false)
                foreach (var tc in config.TwitchConfigs)
                {
                    var t = new TwitchInterface.TwitchInterface();
                    await t.Init(tc);
                    ProtocolInterfaces.ProtocolList.twitchs.Add(t);
                }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}