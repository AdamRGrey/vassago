namespace vassago
{
    using Microsoft.EntityFrameworkCore;
    using vassago;
    using vassago.Models;
    using vassago.TwitchInterface;

    internal class ConsoleService : IHostedService
    {

        public ConsoleService(IConfiguration aspConfig)
        {
            Shared.DBConnectionString = aspConfig["DBConnectionString"];
            DiscordTokens = aspConfig.GetSection("DiscordTokens").Get<IEnumerable<string>>();
            TwitchConfigs = aspConfig.GetSection("TwitchConfigs").Get<IEnumerable<TwitchConfig>>();
            Conversion.Converter.Load(aspConfig["ExchangePairsLocation"]);
        }

        IEnumerable<string> DiscordTokens { get; }
        IEnumerable<TwitchConfig> TwitchConfigs { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var dbc = new ChattingContext();
            await dbc.Database.EnsureCreatedAsync();
            await dbc.Database.MigrateAsync();

            if (DiscordTokens?.Any() ?? false)
                foreach (var dt in DiscordTokens)
                {
                    var d = new DiscordInterface.DiscordInterface();
                    await d.Init(dt);
                    ProtocolInterfaces.ProtocolList.discords.Add(d);
                }

            if (TwitchConfigs?.Any() ?? false)
                foreach (var tc in TwitchConfigs)
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