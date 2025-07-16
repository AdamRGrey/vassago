namespace vassago
{
    using Microsoft.EntityFrameworkCore;
    using vassago;
    using vassago.Models;
    using vassago.ProtocolInterfaces;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    internal class ConsoleService : BackgroundService
    {
        public ConsoleService(IConfiguration aspConfig)
        {
            Shared.DBConnectionString = aspConfig["DBConnectionString"];
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var dbc = new ChattingContext();
            await dbc.Database.MigrateAsync(cancellationToken);

            var confEntity = dbc.Configurations.FirstOrDefault() ?? new Configuration();
            if (dbc.Configurations.Count() == 0)
            {
                dbc.Configurations.Add(confEntity);
                dbc.SaveChanges();
            }
            Console.WriteLine("passing off othe configurator"); //but adam, don't you hate do-nothing classes?
            await Reconfigurator.Initialize(cancellationToken);//yes but this is how I get .net to let me use appsettings.json

            // if (DiscordTokens?.Any() ?? false)
            //     foreach (var dt in DiscordTokens)
            //     {
            //         var d = new DiscordInterface();
            //         initTasks.Add(Task.Run(() =>
            //         {
            //             try
            //             {
            //                 d.Init(dt);
            //                 Shared.ProtocolList.Add(d);
            //             }
            //             catch (Exception e)
            //             {
            //                 Console.Error.WriteLine($"couldn't initialize discord interface with token {dt}");
            //                 Console.Error.WriteLine(e);
            //             }
            //         }));
            //     }
            // if (TwitchConfigs?.Any() ?? false)
            //     foreach (var tc in TwitchConfigs)
            //     {
            //         var t = new TwitchInterface.TwitchInterface();
            //         initTasks.Add(t.Init(tc));
            //         Shared.ProtocolList.Add(t);
            //     }

        }
    }
}
