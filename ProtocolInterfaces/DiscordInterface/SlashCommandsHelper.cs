using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord.WebSocket;
using Discord;
using Discord.Net;

namespace vassago.ProtocolInterfaces
{
    public static class SlashCommandsHelper
    {
        private static List<CommandSetup> slashCommands = new List<CommandSetup>()
        {
            new CommandSetup(){
                Id = "freedomunits",
                UpdatedAt = new DateTime(2023, 5, 21, 13, 3, 0),
                guild = 825293851110801428, //TODO: demagic this magic number
                register = Register_FreedomUnits
            }
        };
        public static async Task Register(DiscordSocketClient client)
        {
            var commandsInContext = await client.GetGlobalApplicationCommandsAsync();
            await Register(client, commandsInContext, null);
            foreach (var guild in client.Guilds)
            {
                try
                {
                    await Register(client, await guild.GetApplicationCommandsAsync(), guild);
                }
                catch (HttpException ex)
                {
                    Console.Error.WriteLine($"error registering slash commands for guild {guild.Name} (id {guild.Id}) - {ex.Message}");
                }
            }
        }

        private static async Task Register(DiscordSocketClient client, IEnumerable<SocketApplicationCommand> commandsInContext, SocketGuild guild)
        {
            foreach (var existingCommand in commandsInContext)
            {
                var myVersion = slashCommands.FirstOrDefault(c => c.Id == existingCommand.Name && c.guild == guild?.Id);
                if (myVersion == null)
                {
                    Console.WriteLine($"deleting command {existingCommand.Name} - (created at {existingCommand.CreatedAt}, it's in guild {existingCommand.Guild?.Id} while I'm in {guild?.Id})");
                    await existingCommand.DeleteAsync();
                }
                else
                {
                    Console.WriteLine(existingCommand.CreatedAt);
                    if (myVersion.UpdatedAt > existingCommand.CreatedAt)
                    {
                        Console.WriteLine($"overwriting command {existingCommand.Name}");
                        await myVersion.register(false, client, guild);
                    }
                    myVersion.alreadyRegistered = true;
                }
            }
            foreach (var remaining in slashCommands.Where(sc => sc.alreadyRegistered == false && sc.guild == guild?.Id))
            {
                Console.WriteLine($"creating new command {remaining.Id} ({(remaining.guild == null ? "global" : $"for guild {remaining.guild}")})");
                await remaining.register(true, client, guild);
            }
        }

        private static async Task Register_FreedomUnits(bool isNew, DiscordSocketClient client, SocketGuild guild)
        {
            var builtCommand = new SlashCommandBuilder()
            .WithName("freedomunits")
            .WithDescription("convert between misc units (currency: iso 4217 code)")
            .AddOption("amount", ApplicationCommandOptionType.Number, "source amount", isRequired: true)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("src-unit")
                .WithDescription("unit converting FROM")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("dest-unit")
                .WithDescription("unit converting TO")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String))
            .Build();
            try
            {
                if (guild != null)
                {
                    if (isNew)
                        await guild.CreateApplicationCommandAsync(builtCommand);
                    else
                        await guild.BulkOverwriteApplicationCommandAsync(new ApplicationCommandProperties[] { builtCommand });
                }
                else
                {
                    if (isNew)
                        await client.CreateGlobalApplicationCommandAsync(builtCommand);
                    else
                        await client.BulkOverwriteGlobalApplicationCommandsAsync(new ApplicationCommandProperties[] { builtCommand });
                }
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.Error.WriteLine(json);
            }
        }
        private class CommandSetup
        {
            public string Id { get; set; }
            //the date/time you updated yours IN UTC.
            public DateTimeOffset UpdatedAt { get; set; }
            public Registration register { get; set; }
            public ulong? guild { get; set; }
            public bool alreadyRegistered {get;set; } = false;

            public delegate Task Registration(bool isNew, DiscordSocketClient client, SocketGuild guild);
        }
    }
}
