using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace silverworker_discord
{
    class Program
    {
        private DiscordSocketClient _client;

        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();
            
        private ISocketMessageChannel botChatterChannel = null;
        private ISocketMessageChannel announcementChannel = null;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, config["token"]);
            await _client.StartAsync();

            _client.MessageReceived += MessageReceived;
            _client.UserJoined += UserJoined;

            _client.Ready += () => Task.Run(() =>{
                Console.WriteLine("Bot is connected!");
                botChatterChannel = _client.GetChannel(ulong.Parse(config["botChatterChannel"])) as ISocketMessageChannel;
                announcementChannel = _client.GetChannel(ulong.Parse(config["announcementChannel"])) as ISocketMessageChannel;
                });
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.Id == _client.CurrentUser.Id) return;

            Console.WriteLine($"{message.Channel}, {message.Content}, {message.Id}");
            if (message.Channel.Id == botChatterChannel.Id)
            {
                if(message.Attachments?.Count > 0)
                {
                    Console.WriteLine(message.Attachments.Count);
                    foreach (var att in message.Attachments)
                    {
                        Console.WriteLine(att.Url);
                        await WebRequest.Create("http://192.168.1.151:3001/shortcuts?display_url=" + att.Url).GetResponseAsync();
                    }
                }
            }
        }
        private Task UserJoined(SocketGuildUser arg)
        {
            Console.WriteLine($"user joined: {arg.Nickname}. Guid: {arg.Guild.Id}. Channel: {arg.Guild.DefaultChannel}");
            var abbreviatedNickname = arg.Nickname;
            if(arg.Nickname.Length > 3){
                abbreviatedNickname = arg.Nickname.Substring(0, arg.Nickname.Length / 3);
            }
            Console.WriteLine($"imma call him {abbreviatedNickname}");
            return arg.Guild.DefaultChannel.SendMessageAsync($"oh hey {abbreviatedNickname}- IPLAYTHESEALOFORICHALCOS <:ORICHALCOS:852749196633309194>");
        }
    }
}