namespace vassago
{
    using franz;
    using Microsoft.EntityFrameworkCore;
    using vassago;
    using vassago.Models;
    using vassago.TwitchInterface;
    using vassago.ProtocolInterfaces.DiscordInterface;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    internal class ConsoleService : BackgroundService
    {
        public ConsoleService(IConfiguration aspConfig)
        {
            Shared.DBConnectionString = aspConfig["DBConnectionString"];
        }

        List<string> DiscordTokens;
        List<TwitchConfig> TwitchConfigs;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var initTasks = new List<Task>();
            var dbc = new ChattingContext();
            await dbc.Database.MigrateAsync(cancellationToken);

            var confEntity = dbc.Configurations.FirstOrDefault() ?? new Configuration();
            if (dbc.Configurations.Count() == 0)
            {
                dbc.Configurations.Add(confEntity);
                dbc.SaveChanges();
            }
            dbConfig(ref confEntity);

            if (DiscordTokens?.Any() ?? false)
                foreach (var dt in DiscordTokens)
                {
                    var d = new DiscordInterface();
                    initTasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            d.Init(dt);
                            Shared.ProtocolList.Add(d);
                        }
                        catch (Exception e){
                            Console.Error.WriteLine($"couldn't initialize discord interface with token {dt}");
                            Console.Error.WriteLine(e);
                        }
                    }));
                }
            if (TwitchConfigs?.Any() ?? false)
                foreach (var tc in TwitchConfigs)
                {
                    var t = new TwitchInterface.TwitchInterface();
                    initTasks.Add(t.Init(tc));
                    Shared.ProtocolList.Add(t);
                }

            Task.WaitAll(initTasks.ToArray(), cancellationToken);
            Console.WriteLine("init tasks are done");
        }
        private void dbConfig(ref vassago.Models.Configuration confEntity)
        {
            Shared.SetupSlashCommands = confEntity.SetupDiscordSlashCommands;
            Shared.API_URL = new Uri(confEntity.reportedApiUrl);
            DiscordTokens = confEntity.DiscordTokens;
            TwitchConfigs = new List<TwitchConfig>();
            if (confEntity.TwitchConfigs != null) foreach (var twitchConfString in confEntity.TwitchConfigs)
                {
                    TwitchConfigs.Add(JsonConvert.DeserializeObject<TwitchConfig>(twitchConfString));
                }
            Conversion.Converter.Load(confEntity.ExchangePairsLocation);
            Telefranz.Configure(confEntity.KafkaName, confEntity.KafkaBootstrap);
            vassago.Behavior.Webhook.SetupWebhooks();
        }
    }
}
