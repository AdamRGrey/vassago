namespace vassago
{
    using franz;
    using Microsoft.EntityFrameworkCore;
    using vassago;
    using vassago.Models;
    using vassago.TwitchInterface;
    using vassago.ProtocolInterfaces.DiscordInterface;
    using System.Runtime.CompilerServices;

    internal class ConsoleService : BackgroundService
    {
        public ConsoleService(IConfiguration aspConfig)
        {
            Shared.DBConnectionString = aspConfig["DBConnectionString"];
            Shared.SetupSlashCommands = aspConfig["SetupSlashCommands"]?.ToLower() == "true";
            Shared.API_URL = new Uri(aspConfig["API_URL"]);
            DiscordTokens = aspConfig.GetSection("DiscordTokens").Get<IEnumerable<string>>();
            TwitchConfigs = aspConfig.GetSection("TwitchConfigs").Get<IEnumerable<TwitchConfig>>();
            Conversion.Converter.Load(aspConfig["ExchangePairsLocation"]);
            Telefranz.Configure(aspConfig["KafkaName"], aspConfig["KafkaBootstrap"]);
            Console.WriteLine($"Telefranz.Configure({aspConfig["KafkaName"]}, {aspConfig["KafkaBootstrap"]});");
            vassago.Behavior.Webhook.SetupWebhooks(aspConfig.GetSection("Webhooks"));
        }

        IEnumerable<string> DiscordTokens { get; }
        IEnumerable<TwitchConfig> TwitchConfigs { get; }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var initTasks = new List<Task>();
            var dbc = new ChattingContext();
            await dbc.Database.MigrateAsync(cancellationToken);

            if (DiscordTokens?.Any() ?? false)
                foreach (var dt in DiscordTokens)
                {
                    var d = new DiscordInterface();
                    initTasks.Add(d.Init(dt));
                    Shared.ProtocolList.Add(d);
                }

            if (TwitchConfigs?.Any() ?? false)
                foreach (var tc in TwitchConfigs)
                {
                    var t = new TwitchInterface.TwitchInterface();
                    initTasks.Add(t.Init(tc));
                    Shared.ProtocolList.Add(t);
                }
            
            Task.WaitAll(initTasks.ToArray(), cancellationToken);
        }

    }
}
