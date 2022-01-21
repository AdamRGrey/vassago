//https://discord.com/oauth2/authorize?client_id=913003037348491264&permissions=274877942784
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
using System.Threading;
using System.Diagnostics;

namespace silverworker_discord
{
    class Program
    {
        private DiscordSocketClient _client;
        private Random r = new Random();

        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        public async Task MainAsync()
        {
            Process[] processes = Process.GetProcesses();
            Process currentProc = Process.GetCurrentProcess();
            Console.WriteLine("Current proccess: {0}", currentProc.ProcessName);
            foreach (Process process in processes)
            {
                if (currentProc.ProcessName == process.ProcessName && currentProc.Id != process.Id)
                {
                    Console.Error.WriteLine($"{DateTime.Now} - Another instance of this process is already running: {process.Id}");
                    return;
                }
            }

            _client = new DiscordSocketClient();

            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, config["token"]);
            await _client.StartAsync();

            _client.Ready += () => Task.Run(() =>
            {
                Console.WriteLine("Bot is connected! going to sign up for message received and user joined in client ready");

                _client.MessageReceived += MessageReceived;
                _client.UserJoined += UserJoined;
            });
            // Block this task until the program is closed.
            await Task.Delay(-1);

        }

#pragma warning disable 4014 //the "you're not awaiting this" warning. yeah I know, that's the beauty of an async method lol
#pragma warning disable 1998 //the "it's async but you're not awaiting anything".
        private async Task MessageReceived(SocketMessage messageParam)
#pragma warning restore 1998
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.Id == _client.CurrentUser.Id) return;

            Console.WriteLine($"#{message.Channel}[{DateTime.Now}][{message.Author.Username} [id={message.Author.Id}]][msg id: {message.Id}] {message.Content}");

            if (message.Author.IsWebhook || message.Author.IsBot)
            {
                if(message.Author.Id == 159985870458322944) //MEE6
                {
                    if(message.Content?.Contains("you just advanced") == true)
                    {
                        var newText = Regex.Replace(message.Content, "<[^>]*>", message.Author.Username);
                        newText = Regex.Replace(newText, "level [\\d]+", "level -1");
                        Features.mock(newText, message);
                    }
                }
            }
            else
            {
                var contentWithoutMention = message.Content;
                var mentionedMe = false;
                if (message.MentionedUsers?.FirstOrDefault(muid => muid.Id == _client.CurrentUser.Id) != null)
                {
                    var mentionOfMe = "<@!" + _client.CurrentUser.Id + ">";
                    contentWithoutMention = message.Content.Replace(mentionOfMe + " ", null);
                    contentWithoutMention = contentWithoutMention.Replace(mentionOfMe, null);
                    mentionedMe = true;
                }
                var wordLikes = message.Content.Split(' ', StringSplitOptions.TrimEntries);
                var links = wordLikes?.Where(wl => Uri.IsWellFormedUriString(wl, UriKind.Absolute)).Select(wl => new Uri(wl));
                if (links != null && links.Count() > 0)
                {
                    foreach (var link in links)
                    {
                        if (link.Host == "vm.tiktok.com")
                        {
                            Features.detiktokify(link, message);
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
                            Features.deheic(message, att);
                            appleReactions = true;
                        }
                    }
                    if (appleReactions)
                    {
                        message.AddReactionAsync(new Emoji("\U0001F34F"));
                    }
                }

                var msgText = message.Content?.ToLower();
                if (!string.IsNullOrWhiteSpace(msgText))
                {

                    if (Regex.IsMatch(msgText, "\\bcloud( |-)?native\\b", RegexOptions.IgnoreCase) ||
                       Regex.IsMatch(msgText, "\\benterprise( |-)?(level|solution)\\b", RegexOptions.IgnoreCase))
                    {
                        switch (r.Next(2))
                        {
                            case 0:
                                await message.AddReactionAsync(new Emoji("\uD83E\uDD2E")); //vomit emoji
                                break;
                            case 1:
                                await message.AddReactionAsync(new Emoji("\uD83C\uDDE7")); //B emoji
                                await message.AddReactionAsync(new Emoji("\uD83C\uDDE6")); //A
                                await message.AddReactionAsync(new Emoji("\uD83C\uDDF3")); //N
                                break;
                        }
                    }
                    if (Regex.IsMatch(msgText, "^(s?he|(yo)?u|y'?all) thinks? i'?m (playin|jokin|kiddin)g?$", RegexOptions.IgnoreCase))
                    {
                        var outgoingMessage = await message.Channel.SendMessageAsync("I believed you for a second, but then you assured me you's a \uD83C\uDDE7   \uD83C\uDDEE   \uD83C\uDDF9   \uD83C\uDDE8   \uD83C\uDDED");
                    }
                    if (Regex.IsMatch(msgText, "\\bskynet\\b", RegexOptions.IgnoreCase))
                    {
                        Features.Skynet(message);
                    }
                    if (Regex.IsMatch(msgText, "\\bi need (an? )?(peptalk|inspiration|ego-?boost)\\b", RegexOptions.IgnoreCase))
                    {
                        Console.WriteLine("peptalk");
                        Features.peptalk(message);
                    }
                    if (Regex.IsMatch(msgText, "\\bwish me luck\\b", RegexOptions.IgnoreCase))
                    {
                        if (r.Next(20) == 0)
                        {
                            await message.AddReactionAsync(new Emoji("\U0001f340"));//4-leaf clover
                        }
                        else
                        {
                            await message.AddReactionAsync(new Emoji("☘️"));
                        }

                    }
                    if (msgText.Contains("!qrplz "))
                    {
                        Features.qrify(msgText.Substring("!qrplz ".Length + msgText.IndexOf("!qrplz ")), message);
                    }
                    if (msgText.Contains("!countdown "))
                    {
                        //Features.countdown(msgText.Substring("!countdown ".Length + msgText.IndexOf("!countdown ")), message); //converting human readable times is hard :/
                    }
                    if (Regex.IsMatch(msgText, "!joke\\b"))
                    {
                        Features.Joke(message);
                    }
                    if (msgText.Contains("cognitive dissonance") == true)
                    {
                        message.ReplyAsync("that's not what cognitive dissonance means.");
                    }
                    if (Regex.IsMatch(msgText, "\\bthank (yo)?u\\b", RegexOptions.IgnoreCase) &&
                    (mentionedMe || Regex.IsMatch(msgText, "\\b(sh?tik)?bot\\b", RegexOptions.IgnoreCase)))
                    {
                        switch (r.Next(4))
                        {
                            case 0:
                                message.Channel.SendMessageAsync("you're welcome, citizen!");
                                break;
                            case 1:
                                message.AddReactionAsync(new Emoji("☺"));
                                break;
                            case 2:
                                message.AddReactionAsync(new Emoji("\U0001F607")); //smiling face with halo
                                break;
                            case 3:
                                switch (r.Next(9))
                                {
                                    case 0:
                                        message.AddReactionAsync(new Emoji("❤")); //normal heart, usually rendered red
                                        break;
                                    case 1:
                                        message.AddReactionAsync(new Emoji("\U0001F9E1")); //orange heart
                                        break;
                                    case 2:
                                        message.AddReactionAsync(new Emoji("\U0001F49B")); //yellow heart
                                        break;
                                    case 3:
                                        message.AddReactionAsync(new Emoji("\U0001F49A")); //green heart
                                        break;
                                    case 4:
                                        message.AddReactionAsync(new Emoji("\U0001F499")); //blue heart
                                        break;
                                    case 5:
                                        message.AddReactionAsync(new Emoji("\U0001F49C")); //purple heart
                                        break;
                                    case 6:
                                        message.AddReactionAsync(new Emoji("\U0001F90E")); //brown heart
                                        break;
                                    case 7:
                                        message.AddReactionAsync(new Emoji("\U0001F5A4")); //black heart
                                        break;
                                    case 8:
                                        message.AddReactionAsync(new Emoji("\U0001F90D")); //white heart
                                        break;
                                }
                                break;
                        }
#pragma warning restore 4014
                    }
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