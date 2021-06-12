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
        private Random r = new Random();

        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();
            
        int initNonce;
        private ISocketMessageChannel targetChannel = null;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        public async Task MainAsync()
        {
            initNonce = r.Next();
            _client = new DiscordSocketClient();

            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, config["token"]);
            await _client.StartAsync();

            _client.MessageReceived += MessageReceived;
            _client.UserJoined += UserJoined;

            _client.Ready += () =>
            {
                Console.WriteLine("Bot is connected! this is the dumbest.");
                var wh = new Discord.Webhook.DiscordWebhookClient(config["initWebhook"]);
                return wh.SendMessageAsync(initNonce.ToString(), username: "silver loop");
            };
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.Id == _client.CurrentUser.Id) return;

            Console.WriteLine($"{message.Channel}, {message.Content}, {message.Id}");
            if(targetChannel == null && 
                message.Author.Username == "silver loop" && message.Content == initNonce.ToString())
            {
                targetChannel = message.Channel;
                Task.WaitAll(message.DeleteAsync(), targetChannel.SendMessageAsync("this initialization is nonsense lol"));
            }
            else if (message.Channel.Id != targetChannel.Id)
            {
                return;
            }

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