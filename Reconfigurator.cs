namespace vassago
{
    using vassago.Models;
    using vassago.ProtocolInterfaces;
    using Newtonsoft.Json;
    using franz;

    public static class Reconfigurator
    {
        private static Rememberer r = Rememberer.Instance;
        private static Configuration conf;
        private static List<ProtocolConfiguration> protocolConfigs;
        // private static List<ProtocolDiscord> protocolConfigsDiscord;
        // private static List<ProtocolTwitch> protocolConfigsTwitch;

        public static async Task Initialize(CancellationToken cancellationToken)
        {
            conf = r.Configuration();
            var initTasks = new List<Task>();
            initTasks.Add(Conversions());
            initTasks.Add(Webhooks());
            initTasks.Add(Kafka());
            initTasks.Add(ProtocolInterfaces());
            Task.WaitAll(initTasks.ToArray());
        }

        public static async Task Kafka()
        {
            Telefranz.Configure(conf.KafkaName, conf.KafkaBootstrap);
        }
        public static async Task Webhooks()
        {
            vassago.Behavior.Webhook.SetupWebhooks();
        }
        public static async Task Conversions()
        {
            Conversion.Converter.Load(conf.ExchangePairsLocation);
        }
        public static async Task ProtocolInterfaces()
        {
            var initTasks = new List<Task>();
            var incomingConfigs = r.ProtocolConfigurations();
            var newConfigs = incomingConfigs.ToList();
            var removedConfigs = new List<ProtocolConfiguration>();
            var updatedConfigs = new List<ProtocolConfiguration>();
            // var untouchedConfigs = new List<ProtocolConfiguration>();
            if(protocolConfigs != null) foreach (var oldCfg in protocolConfigs)
            {
                var notActuallyNew = newConfigs.FirstOrDefault(cfg => cfg.Id == oldCfg.Id);
                if (notActuallyNew != null)
                    newConfigs.Remove(notActuallyNew);

                var match = incomingConfigs.FirstOrDefault(cfg => cfg.Id == oldCfg.Id);
                if (match == null)
                {
                    removedConfigs.Add(oldCfg);
                }
                else
                {
                    if (JsonConvert.SerializeObject(oldCfg) == JsonConvert.SerializeObject(match))
                    {
                        // untouchedConfigs.Add(oldCfg);
                    }
                    else
                    {
                        updatedConfigs.Add(match);
                    }
                }
            }

            if (removedConfigs != null) foreach (var removedCfg in removedConfigs)
            {
                var protocolInterface = Shared.ProtocolList.FirstOrDefault(pi => pi.ConfigurationEntity.Id == removedCfg.Id);
                if (protocolInterface == null)
                {
                    Console.Error.WriteLine($"attempting to remove interface for {removedCfg.Id}, but not found as set-up entity!");
                    continue;
                }
                protocolInterface.Die();
                Shared.ProtocolList.Remove(protocolInterface);
            }

            if(updatedConfigs != null) foreach (var updatedCfg in updatedConfigs)
            {
                var protocolInterface = Shared.ProtocolList.FirstOrDefault(pi => pi.ConfigurationEntity.Id == updatedCfg.Id);
                if (protocolInterface == null)
                {
                    Console.Error.WriteLine($"attempting to update interface for {updatedCfg.Id}, but not found as set-up entity!");
                    continue;
                }
                protocolInterface.UpdateConfiguration(updatedCfg);
            }
            if(newConfigs != null) foreach (var newCfg in newConfigs)
            {
                var protocolInterface = Shared.ProtocolList.FirstOrDefault(pi => pi.ConfigurationEntity.Id == newCfg.Id);
                if (protocolInterface != null)
                {
                    Console.Error.WriteLine($"attempting to create interface for {newCfg.Id}, but already found as set-up entity!");
                    protocolInterface.UpdateConfiguration(newCfg);
                    continue;
                }
                //TODO: get EFCore to give me the subtype
                switch (newCfg.Protocol)
                {
                    case "discord":
                        var d = new DiscordInterface();
                        initTasks.Add(d.Init(newCfg as ProtocolDiscord));
                        Shared.ProtocolList.Add(d);
                        break;
                    case "twitch":
                        var t = new TwitchInterface();
                        initTasks.Add(t.Init(newCfg as ProtocolTwitch));
                        Shared.ProtocolList.Add(t);
                        break;
                    default:
                        Console.Error.WriteLine($"attempting to create interface for {newCfg.Id}, but can't figure out what to do with {newCfg.Protocol}!");
                        break;
                }
            }
            protocolConfigs = incomingConfigs;
            Task.WaitAll(initTasks.ToArray());
        }
    }
}
