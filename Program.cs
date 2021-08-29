using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

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


            _client.Ready += () => Task.Run(() =>
            {
                Console.WriteLine("Bot is connected! going to sign up for message received and user joined in client ready");
                botChatterChannel = _client.GetChannel(ulong.Parse(config["botChatterChannel"])) as ISocketMessageChannel;
                announcementChannel = _client.GetChannel(ulong.Parse(config["announcementChannel"])) as ISocketMessageChannel;

                _client.MessageReceived += MessageReceived;
                _client.UserJoined += UserJoined;
            });
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.Id == _client.CurrentUser.Id) return;

            Console.WriteLine($"{message.Channel}, {message.Content} (message id: {message.Id})");

            if (message.Author.IsWebhook)
            {
                if (message.Author.Username == "greasemonkey reward watcher")
                {
                    Console.WriteLine("yep");
                    var redemptionData = message.Content.Split("\n")[1].Substring("data: ".Length);

                    if (message.Content.StartsWith("type: reward-request"))
                    {
                        var components = redemptionData.Split("•");
                        Console.WriteLine($"{components.Length} components:");
                        var rewardName = components[0].Trim();
                        var redeemer = components[1].Trim();
                        var textData = "";
                        if (components[1].Contains(":"))
                        {
                            redeemer = components[1].Substring(0, components[1].IndexOf(":")).Trim();
                            textData = components[1].Substring(components[1].IndexOf(":")).Trim();
                        }
                        Console.WriteLine($"user: {redeemer} redeems {rewardName}, text data? {textData}");

                        var redemptionSerialized = Encoding.ASCII.GetBytes(
                            JsonConvert.SerializeObject(new
                            {
                                redeemer = redeemer,
                                rewardName = rewardName,
                                textData = textData
                            }, Formatting.None));
                        var wr = WebRequest.Create("http://192.168.1.151:3001/shortcuts/redeemReward");
                        wr.Method = "POST";
                        wr.ContentType = "application/json";
                        wr.ContentLength = redemptionSerialized.Length;
                        using (var postStream = wr.GetRequestStream())
                        {
                            postStream.Write(redemptionSerialized);
                        }
                        await wr.GetResponseAsync();
                    }
                }
            }
            else
            {
                if (message.Channel.Id == botChatterChannel.Id)
                {
                    if (message.Attachments?.Count > 0)
                    {
                        Console.WriteLine(message.Attachments.Count);
                        foreach (var att in message.Attachments)
                        {
                            Console.WriteLine(att.Url);
                            await WebRequest.Create("http://192.168.1.151:3001/shortcuts?display_url=" + att.Url).GetResponseAsync();
                        }
                    }
                }
                else
                {
                    //any channel, from a user
                    var wordLikes = message.Content.Split(' ', StringSplitOptions.TrimEntries);
                    var links = wordLikes?.Where(wl => Uri.IsWellFormedUriString(wl, UriKind.Absolute)).Select(wl => new Uri(wl));
                    if (links != null && links.Count() > 0)
                    {
                        foreach (var link in links)
                        {
                            if (link.Host == "vm.tiktok.com")
                            {
                                detiktokify(link, message.Channel);
                            }
                        }
                    }
                }
            }
        }
        private async void detiktokify(Uri link, ISocketMessageChannel channel)
        {
            var ytdl = new YoutubeDLSharp.YoutubeDL();
            ytdl.YoutubeDLPath = config["ytdl"];
            ytdl.FFmpegPath = "ffmpeg";
            ytdl.OutputFolder = config["content_wasteland"];
            var res = await ytdl.RunVideoDownload(link.ToString());
            string path = res.Data;
            await channel.SendFileAsync(path);
            File.Delete(path);
        }
        private Task UserJoined(SocketGuildUser arg)
        {
            Console.WriteLine($"user joined: {arg.Nickname}. Guid: {arg.Guild.Id}. Channel: {arg.Guild.DefaultChannel}");
            var abbreviatedNickname = arg.Nickname;
            if (arg.Nickname.Length > 3)
            {
                abbreviatedNickname = arg.Nickname.Substring(0, arg.Nickname.Length / 3);
            }
            Console.WriteLine($"imma call him {abbreviatedNickname}");
            return arg.Guild.DefaultChannel.SendMessageAsync($"oh hey {abbreviatedNickname}- IPLAYTHESEALOFORICHALCOS <:ORICHALCOS:852749196633309194>");
        }
    }
}