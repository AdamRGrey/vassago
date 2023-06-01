//https://discord.com/oauth2/authorize?client_id=913003037348491264&permissions=274877942784&scope=bot%20messages.read
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
 using vassago.Models;
using vassago.DiscordInterface.Models;
using vassago.Behavior;

namespace vassago.DiscordInterface;

public class DiscordInterface
{
    private DiscordSocketClient _client;
    private DiscordProtocol protocolInterface;
    private bool eventsSignedUp = false;
    private ChattingContext _db;
    public DiscordInterface()
    {
        _db = Shared.dbContext;
    }

    public async Task Init(string token)
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All });

        _client.Log += (msg) =>
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        };

        _client.Ready += () => Task.Run(() =>
        {
            if (!eventsSignedUp)
            {
                eventsSignedUp = true;
                Console.WriteLine("Bot is connected! going to sign up for message received and user joined in client ready");

                _client.MessageReceived += MessageReceived;
                // _client.MessageUpdated +=
                _client.UserJoined += UserJoined;
                _client.SlashCommandExecuted += SlashCommandHandler;
                // _client.ChannelCreated +=
                // _client.ChannelDestroyed +=
                // _client.ChannelUpdated +=
                // _client.GuildMemberUpdated +=
                // _client.UserBanned +=
                // _client.UserLeft +=
                // _client.ThreadCreated += 
                // _client.ThreadUpdated += 
                // _client.ThreadDeleted +=
                // _client.JoinedGuild +=
                // _client.GuildUpdated += 
                // _client.LeftGuild += 
                
                SlashCommandsHelper.Register(_client).GetAwaiter().GetResult();
            }
            else
            {
                Console.WriteLine("bot appears to be RE connected, so I'm not going to sign up twice");
            }
        });

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
    }
#pragma warning disable 4014 //the "you're not awaiting this" warning. yeah I know, that's the beauty of an async method lol
#pragma warning disable 1998 //the "it's async but you're not awaiting anything".
    private async Task MessageReceived(SocketMessage messageParam)
#pragma warning restore 1998
    {
        var suMessage = messageParam as SocketUserMessage;
        if (suMessage == null) return;

        var m = _db.Messages.FirstOrDefault(mi => mi.ExternalId == suMessage.Id) as DiscordMessage;
        if(m == null)
        {
            m = _db.Messages.Add(new DiscordMessage(suMessage)).Entity as DiscordMessage;
        }
        m.Intake(suMessage, _client.CurrentUser.Id);
        
        m.Channel = UpsertChannel(suMessage.Channel);
        m.Author = UpsertUser(suMessage.Author);
        _db.SaveChanges();
        Console.WriteLine($"#{suMessage.Channel}[{DateTime.Now}][{suMessage.Author.Username} [id={suMessage.Author.Id}]][msg id: {suMessage.Id}] {suMessage.Content}");
        if (suMessage.Author.Id == _client.CurrentUser.Id) return;


        if (suMessage.MentionedUsers?.FirstOrDefault(muid => muid.Id == _client.CurrentUser.Id) != null)
        {
            var mentionOfMe = "<@" + _client.CurrentUser.Id + ">";
            m.MentionsMe = true;
        }

        //TODO: standardize content

        if(await thingmanagementdoer.Instance.ActOn(m))
        {
            m.ActedOn = true;
            _db.SaveChanges();
        }
    }

    private Task UserJoined(SocketGuildUser arg)
    {
        var guild = UpsertChannel(arg.Guild);
        var defaultChannel = UpsertChannel(arg.Guild.DefaultChannel);
        defaultChannel.ParentChannel = guild;
        var u = UpsertUser(arg);
        if(u.SeenInChannels == null) u.SeenInChannels = new List<Channel>();
        var sighting = u.SeenInChannels?.FirstOrDefault(c => c.ExternalId == arg.Guild.Id);
        if(sighting == null)
        {
            var seenIn = u.SeenInChannels as List<Channel>;
            seenIn.Add(guild);
            seenIn.Add(defaultChannel);
            u.SeenInChannels = seenIn;
            _db.SaveChanges();
        }
        return thingmanagementdoer.Instance.OnJoin(u, defaultChannel);

        // Console.WriteLine($"user joined: {arg.Nickname}. Guid: {arg.Guild.Id}. Channel: {arg.Guild.DefaultChannel}");
        // var abbreviatedNickname = arg.Nickname;
        // if (arg.Nickname.Length > 3)
        // {
        //     abbreviatedNickname = arg.Nickname.Substring(0, arg.Nickname.Length / 3);
        // }
        // Console.WriteLine($"imma call him {abbreviatedNickname}");
        // return arg.Guild.DefaultChannel.SendMessageAsync($"oh hey {abbreviatedNickname}- IPLAYTHESEALOFORICHALCOS <:ORICHALCOS:852749196633309194>");
    }
    private async Task ButtonHandler(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "custom-id":
                await component.RespondAsync($"{component.User.Mention}, it's been here the whole time!");
                break;
        }
    }
    internal static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.CommandName)
        {
            case "freedomunits":
                try
                {
                    var amt = Convert.ToDecimal((double)(command.Data.Options.First(o => o.Name == "amount").Value));
                    var src = (string)command.Data.Options.First(o => o.Name == "src-unit").Value;
                    var dest = (string)command.Data.Options.First(o => o.Name == "dest-unit").Value;
                    var conversionResult = Conversion.Converter.Convert(amt, src, dest);

                    await command.RespondAsync($"> {amt} {src} -> {dest}\n{conversionResult}");
                }
                catch (Exception e)
                {
                    await command.RespondAsync($"error: {e.Message}. aaadam!");
                }
                break;
            default:
                await command.RespondAsync($"\\*smiles and nods*\n");
                await command.Channel.SendFileAsync($"assets/loud sweating.gif");
                Console.Error.WriteLine($"can't understand command name: {command.CommandName}");
                break;
        }
    }

    private Channel UpsertChannel(ISocketMessageChannel channel)
    {
        var c = _db.Channels.FirstOrDefault(ci => ci.ExternalId == channel.Id);
        if(c == null)
        {
            c = _db.Channels.Add(new DiscordChannel()).Entity;
            _db.SaveChanges();
        }
        if(channel is IGuildChannel)
        {
            c.ParentChannel = UpsertChannel((channel as IGuildChannel).Guild);
        }
        else if (channel is IPrivateChannel)
        {
            c.ParentChannel = protocolInterface;
        }
        else
        {
            c.ParentChannel = protocolInterface;
            Console.WriteLine($"trying to upsert channel {channel.Id}/{channel.Name}, but it's neither guildchannel nor private channel. shrug.jpg");
        }
        return c;        
    }
    private Channel UpsertChannel(IGuild channel)
    {
        var c = _db.Channels.FirstOrDefault(ci => ci.ExternalId == channel.Id);
        if(c == null)
        {
            c = _db.Channels.Add(new DiscordChannel()).Entity;
            _db.SaveChanges();
        }
        c.ParentChannel = protocolInterface;
        return c;
    }

    private User UpsertUser(SocketUser user)
    {
        var u = _db.Users.FirstOrDefault(ui => ui.ExternalId == user.Id);
        if(u == null)
        {
            u = _db.Users.Add(new DiscordUser()).Entity;
            _db.SaveChanges();
        }
        return u;
    }
}