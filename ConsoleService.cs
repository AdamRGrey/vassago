namespace vassago
{
    using Microsoft.EntityFrameworkCore;
    using vassago;
    using vassago.Models;
    using vassago.TwitchInterface;
    using vassago.ProtocolInterfaces.DiscordInterface;
    using System.Runtime.CompilerServices;

    internal class ConsoleService : IHostedService
    {
        public ConsoleService(IConfiguration aspConfig)
        {
            Shared.DBConnectionString = aspConfig["DBConnectionString"];
            Shared.SetupSlashCommands = aspConfig["SetupSlashCommands"]?.ToLower() == "true";
            DiscordTokens = aspConfig.GetSection("DiscordTokens").Get<IEnumerable<string>>();
            TwitchConfigs = aspConfig.GetSection("TwitchConfigs").Get<IEnumerable<TwitchConfig>>();
            Conversion.Converter.Load(aspConfig["ExchangePairsLocation"]);
            vassago.Behavior.Webhook.SetupWebhooks(aspConfig.GetSection("Webhooks"));
        }

        IEnumerable<string> DiscordTokens { get; }
        IEnumerable<TwitchConfig> TwitchConfigs { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var initTasks = new List<Task>();
            var dbc = new ChattingContext();
            await dbc.Database.MigrateAsync(cancellationToken);

            if (DiscordTokens?.Any() ?? false)
                foreach (var dt in DiscordTokens)
                {
                    var d = new DiscordInterface();
                    initTasks.Add(d.Init(dt));
                    ProtocolInterfaces.ProtocolList.discords.Add(d);
                }

            if (TwitchConfigs?.Any() ?? false)
                foreach (var tc in TwitchConfigs)
                {
                    var t = new TwitchInterface.TwitchInterface();
                    initTasks.Add(t.Init(tc));
                    ProtocolInterfaces.ProtocolList.twitchs.Add(t);
                }
            
            Task.WaitAll(initTasks.ToArray(), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return null;
        }
    }
}
