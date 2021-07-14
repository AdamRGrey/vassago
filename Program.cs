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


            _client.Ready += () => Task.Run(() =>{
                Console.WriteLine("Bot is connected! going to sign up for message received and user joined in client ready");
                botChatterChannel = _client.GetChannel(ulong.Parse(config["botChatterChannel"])) as ISocketMessageChannel;
                announcementChannel = _client.GetChannel(ulong.Parse(config["announcementChannel"])) as ISocketMessageChannel;

                _client.MessageReceived += MessageReceived;
                _client.UserJoined += UserJoined;
                });
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task redeemReward(string rewardName, string username, string text)
        {
            switch(rewardName)
            {
                case "the seAL OF ORICHALCOS":
                {
                    Console.WriteLine("going to throw up THE SEAL OF ORICHALCOS");
                    await WebRequest.Create("http://192.168.1.151:3001/shortcuts?display_url=/twitchery/SEAL.mp4").GetResponseAsync();
                }
                break;
                case "Timeout":
                case "healthpack":
                case "Way of Anne":
                case "Treasure Token":
                case "Platz Eins":
                case "Proliferate":
                case "go fish":
                case "clear your mind":
                case "Banna Deck":
                case "SPEHSS MEHREENS":
                case "wrath of gob":
                {
                    Console.WriteLine("need thing");
                    await WebRequest.Create("http://192.168.1.151:3001/shortcuts?display_url=/twitchery/placeholder.png").GetResponseAsync();
                }
                break;
                case "literally nothing":
                {
                    //not even acknowledgement
                }
                break;
                default:
                {
                    await botChatterChannel.SendMessageAsync("... dafuq?");
                    break;
                }
            }
        }

        private async Task MessageReceived(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.Id == _client.CurrentUser.Id) return;

            Console.WriteLine($"{message.Channel}, {message.Content} (message id: {message.Id})");

            if(message.Author.IsWebhook)
            {
                if(message.Author.Username == "greasemonkey reward watcher")
                {
                    Console.WriteLine("yep");
                    var redemptionData = message.Content.Split("\n")[1].Substring("data: ".Length);

                    if(message.Content.StartsWith("type: reward-request"))
                    {
                        var components = redemptionData.Split("•");
                        Console.WriteLine($"{components.Length} components:");
                        var rewardName = components[0].Trim();
                        var redeemer = components[1].Trim();
                        var textData = "";
                        if(components[1].Contains(":"))
                        {
                            redeemer = components[1].Substring(0, components[1].IndexOf(":")).Trim();
                            textData = components[1].Substring(components[1].IndexOf(":")).Trim();
                        }
                        Console.WriteLine($"user: {redeemer} redeems {rewardName}, text data? {textData}");
                        await redeemReward(rewardName, redeemer, textData);
                    }
                }
            }
            else
            {
                
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