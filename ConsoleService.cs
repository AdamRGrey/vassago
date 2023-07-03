namespace vassago
{
    using vassago;
    using vassago.Models;

    internal class ConsoleService : IHostedService
    {
        Configuration config = new Configuration();
        private List<DiscordInterface.DiscordInterface> discords = new List<DiscordInterface.DiscordInterface>();

        public ConsoleService(IConfiguration aspConfig)
        {
            config.DBConnectionString = aspConfig["DBConnectionString"];
            config.ExchangePairsLocation = aspConfig["ExchangePairsLocation"];
            config.DiscordTokens = aspConfig.GetSection("DiscordTokens").Get<IEnumerable<string>>();
            //config.TwitchTokens = aspConfig["TwitchTokens"];
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Shared.DBConnectionString = config.DBConnectionString;
            var dbc = new ChattingContext();
            dbc.Database.EnsureCreated();

            Conversion.Converter.Load(config.ExchangePairsLocation);
            if (config.DiscordTokens.Any())
                foreach (var dt in config.DiscordTokens)
                {
                    var d = new DiscordInterface.DiscordInterface();
                    await d.Init(dt);
                    discords.Add(d);
                }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}