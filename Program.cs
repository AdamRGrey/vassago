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

namespace silverworker_discord
{
    class Program
    {
        private DiscordSocketClient _client;

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

#pragma warning disable 1998 //the "it's async but you're not awaiting anything".
        private async Task MessageReceived(SocketMessage messageParam)
#pragma warning restore 1998
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.Id == _client.CurrentUser.Id) return;

            Console.WriteLine($"{message.Channel}, {message.Content} (message id: {message.Id})");

            if (message.Author.IsWebhook)
            {
            }
            else
            {
                //any channel, from a user
                if (message.Content.StartsWith("!qrplz "))
                {
                    Features.qrify(message.Content.Substring("!qrplz ".Length), message);
                }
                else
                {
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
#pragma warning disable 4014 //the "you're not awaiting this" warning. yeah I know, that's the beauty of an async method lol
                            message.AddReactionAsync(new Emoji("\U0001F34F"));
#pragma warning restore 4014
                        }
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