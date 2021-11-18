//https://discord.com/api/oauth2/authorize?client_id={application id}&permissions=0&scope=bot%20applications.commands
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
using ImageMagick;
using ImageMagick.Formats;

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
        private ISocketMessageChannel mtgChannel = null;

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
                mtgChannel = _client.GetChannel(ulong.Parse(config["mtgChannel"])) as ISocketMessageChannel;

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
                    Console.WriteLine("heard greasemonkey, this is bananas");
                    var type = message.Content.Split("\n")[0].Substring("type: ".Length);
                    var subData = message.Content.Split("\n")[1].Substring("data: ".Length);
                    try
                    {
                        await twitchery.twitcherize(type, subData);
                    }
                    catch (Exception e)
                    {
                        await botChatterChannel.SendMessageAsync($"aaaadam!\n{JsonConvert.SerializeObject(e)}");
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
                                detiktokify(link, message);
                            }
                        }
                    }

                    if (message.Attachments?.Count > 0)
                    {
                        Console.WriteLine($"{message.Attachments.Count} attachments");
                        var appleReactions = false;
                        foreach (var att in message.Attachments)
                        {
                            if (att.Filename?.EndsWith(".heic") == true)
                            {
                                deheic(message, att);
                                appleReactions = true;
                            }
                        }
                        if (appleReactions)
                        {
                            #pragma warning disable 4014 //the "you're not awaiting this" warning. yeah I know, that's the beauty of an async method lol
                            message.AddReactionAsync(new Emoji("\U0001F34F"));
                            #pragma warning restore 4014
                        }
                    }
                }
            }
        }
        private async void deheic(SocketUserMessage message, Attachment att)
        {
            try
            {
                var request = WebRequest.Create(att.Url);
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (var convertedStream = new MemoryStream())
                using (var image = new MagickImage(response.GetResponseStream()))
                {
                    image.Write(convertedStream, MagickFormat.Jpeg);
                    convertedStream.Position = 0;
                    Console.WriteLine($"converted to stream {convertedStream.Length} bytes long");
                    await message.Channel.SendFileAsync(convertedStream, "heiccup.jpg", "converted from jpeg-but-apple to jpeg");
                }
            }

            catch (Exception e)
            {
                await message.Channel.SendMessageAsync("¯\\_(ツ)_/¯");
                Console.Error.Write(e);
            }
        }
        private async void detiktokify(Uri link, SocketUserMessage message)
        {
            var ytdl = new YoutubeDLSharp.YoutubeDL();
            ytdl.YoutubeDLPath = "youtube-dl";
            ytdl.FFmpegPath = "ffmpeg";
            ytdl.OutputFolder = "";
            ytdl.OutputFileTemplate = "tiktokbad.%(ext)s";
            var res = await ytdl.RunVideoDownload(link.ToString());
            if (!res.Success)
            {
                Console.Error.WriteLine("tried to dl, failed. \n" + string.Join('\n', res.ErrorOutput));
                await message.AddReactionAsync(Emote.Parse("<:problemon:859453047141957643>"));
                await botChatterChannel.SendMessageAsync("tried to dl, failed. \n" + string.Join('\n', res.ErrorOutput));
            }
            else
            {
                string path = res.Data;
                if (File.Exists(path))
                {
                    try
                    {
                        await message.Channel.SendFileAsync(path);
                    }
                    catch (Exception e)
                    {
                        await botChatterChannel.SendMessageAsync($"aaaadam!\n{JsonConvert.SerializeObject(e)}");
                    }
                    File.Delete(path);
                }
                else
                {
                    Console.Error.WriteLine("idgi but something happened.");
                    await message.AddReactionAsync(Emote.Parse("<:problemon:859453047141957643>"));
                }
            }
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