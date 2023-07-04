//https://discord.com/oauth2/authorize?client_id=913003037348491264&permissions=274877942784&scope=bot%20messages.read
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using vassago.Models;
using vassago.Behavior;
using Discord.Rest;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Reactive.Linq;

namespace vassago.DiscordInterface;

public class DiscordInterface
{
    internal const string PROTOCOL = "discord";
    internal DiscordSocketClient client;
    private bool eventsSignedUp = false;
    private ChattingContext _db;
    private static SemaphoreSlim discordChannelSetup = new SemaphoreSlim(1, 1);
    private Channel protocolAsChannel;

    public DiscordInterface()
    {
        _db = new ChattingContext();
    }

    public async Task Init(string token)
    {
        await SetupDiscordChannel();
        client = new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All });

        client.Log += (msg) =>
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        };
        client.Connected += SelfConnected;
        client.Ready += ClientReady;

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
    }

    private async Task SetupDiscordChannel()
    {
        await discordChannelSetup.WaitAsync();

        try
        {
            protocolAsChannel = _db.Channels.FirstOrDefault(c => c.ParentChannel == null && c.Protocol == PROTOCOL);
            if (protocolAsChannel == null)
            {
                protocolAsChannel = new Channel()
                {
                    DisplayName = "discord (itself)",
                    Permissions = new PermissionSettings()
                    {
                        MeannessFilterLevel = Enumerations.MeannessFilterLevel.Strict,
                        LewdnessFilterLevel = Enumerations.LewdnessFilterLevel.Moderate,
                        MaxTextChars = 2000,
                        MaxAttachmentBytes = 25 * 1024 * 1024, //allegedly it's 25, but I worry it's not actually.
                        LinksAllowed = true,
                        ReactionsPossible = true
                    },
                    ExternalId = null,
                    Protocol = PROTOCOL,
                    SubChannels = new List<Channel>()
                };
                protocolAsChannel.SendMessage = (t) => { throw new InvalidOperationException($"discord itself cannot accept text"); };
                protocolAsChannel.SendFile = (f, t) => { throw new InvalidOperationException($"discord itself cannot send file"); };
                _db.Channels.Add(protocolAsChannel);
                _db.SaveChanges();
            }
        }
        finally
        {
            discordChannelSetup.Release();
        }
    }

    private async Task ClientReady()
    {
        if (!eventsSignedUp)
        {
            eventsSignedUp = true;
            Console.WriteLine($"Bot is connected ({client.CurrentUser.Username}; {client.CurrentUser.Mention})! going to sign up for message received and user joined in client ready");

            client.MessageReceived += MessageReceived;
            // _client.MessageUpdated +=
            //client.UserJoined += UserJoined;
            client.SlashCommandExecuted += SlashCommandHandler;
            //client.ChannelCreated +=
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

            await SlashCommandsHelper.Register(client);
        }
        else
        {
            Console.WriteLine("bot appears to be RE connected, so I'm not going to sign up twice");
        }
    }

    private async Task SelfConnected()
    {
        var selfUser = UpsertAccount(client.CurrentUser);
        selfUser.SeenInChannel = protocolAsChannel;
        selfUser.DisplayName = client.CurrentUser.Username;

        await _db.SaveChangesAsync();
        Behaver.Instance.Selves.Add(selfUser);
    }

    private async Task MessageReceived(SocketMessage messageParam)
    {
        var suMessage = messageParam as SocketUserMessage;
        if (suMessage == null)
        {
            Console.WriteLine($"{messageParam.Content}, but not a user message");
            return;
        }
        Console.WriteLine($"#{suMessage.Channel}[{DateTime.Now}][{suMessage.Author.Username} [id={suMessage.Author.Id}]][msg id: {suMessage.Id}] {suMessage.Content}");

        var m = UpsertMessage(suMessage);

        if (suMessage.MentionedUsers?.FirstOrDefault(muid => muid.Id == client.CurrentUser.Id) != null)
        {
            var mentionOfMe = "<@" + client.CurrentUser.Id + ">";
            m.MentionsMe = true;
        }
        if (await Behaver.Instance.ActOn(m))
        {
            m.ActedOn = true;
        }
        _db.SaveChanges();
    }

    private void UserJoined(SocketGuildUser arg)
    {
        var guild = UpsertChannel(arg.Guild);
        var defaultChannel = UpsertChannel(arg.Guild.DefaultChannel);
        defaultChannel.ParentChannel = guild;
        var u = UpsertAccount(arg);
        u.SeenInChannel = guild;
        u.DisplayName = arg.DisplayName;
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
    internal vassago.Models.Attachment UpsertAttachment(IAttachment dAttachment)
    {
        var a = _db.Attachments.FirstOrDefault(ai => ai.ExternalId == dAttachment.Id);
        if (a == null)
        {
            a = new vassago.Models.Attachment();
            _db.Attachments.Add(a);
        }
        a.ContentType = dAttachment.ContentType;
        a.Description = dAttachment.Description;
        a.Filename = dAttachment.Filename;
        a.Size = dAttachment.Size;
        a.Source = new Uri(dAttachment.Url);

        return a;
    }
    internal Message UpsertMessage(IUserMessage dMessage)
    {
        var m = _db.Messages.FirstOrDefault(mi => mi.ExternalId == dMessage.Id.ToString() && mi.Protocol == PROTOCOL);
        if (m == null)
        {
            m = new Message();
            _db.Messages.Add(m);
        }
        m.Attachments = m.Attachments ?? new List<vassago.Models.Attachment>();
        if (dMessage.Attachments?.Any() == true)
        {
            m.Attachments = new List<vassago.Models.Attachment>();
            foreach (var da in dMessage.Attachments)
            {
                m.Attachments.Add(UpsertAttachment(da));
            }
        }
        m.Content = dMessage.Content;
        m.ExternalId = dMessage.Id.ToString();
        m.Timestamp = dMessage.EditedTimestamp ?? dMessage.CreatedAt;
        m.Channel = UpsertChannel(dMessage.Channel);
        m.Author = UpsertAccount(dMessage.Author);
        m.Author.SeenInChannel = m.Channel;
        if(dMessage.Channel is IGuildChannel)
        {
            m.Author.DisplayName = (dMessage.Author as IGuildUser).DisplayName;//discord forgot how display names work.
        }
        m.MentionsMe = (dMessage.Author.Id != client.CurrentUser.Id
            && (dMessage.MentionedUserIds?.FirstOrDefault(muid => muid == client.CurrentUser.Id) > 0));

        m.Reply = (t) => { return dMessage.ReplyAsync(t); };
        m.React = (e) => { return attemptReact(dMessage, e); };
        return m;
    }
    internal Channel UpsertChannel(IMessageChannel channel)
    {
        Channel c = _db.Channels.FirstOrDefault(ci => ci.ExternalId == channel.Id.ToString() && ci.Protocol == PROTOCOL);
        if (c == null)
        {
            c = new Channel();
            _db.Channels.Add(c);
        }

        c.DisplayName = channel.Name;
        c.ExternalId = channel.Id.ToString();
        c.IsDM = channel is IPrivateChannel;
        c.Messages = c.Messages ?? new List<Message>();
        c.Protocol = PROTOCOL;
        if (channel is IGuildChannel)
        {
            c.ParentChannel = UpsertChannel((channel as IGuildChannel).Guild);
            c.ParentChannel.SubChannels.Add(c);
        }
        else if (channel is IPrivateChannel)
        {
            c.ParentChannel = protocolAsChannel;
        }
        else
        {
            c.ParentChannel = protocolAsChannel;
            Console.Error.WriteLine($"trying to upsert channel {channel.Id}/{channel.Name}, but it's neither guildchannel nor private channel. shrug.jpg");
        }
        c.SubChannels = c.SubChannels ?? new List<Channel>();
        c.SendMessage = (t) => { return channel.SendMessageAsync(t); };
        c.SendFile = (f, t) => { return channel.SendFileAsync(f, t); };
        return c;
    }
    internal Channel UpsertChannel(IGuild channel)
    {
        Channel c = _db.Channels.FirstOrDefault(ci => ci.ExternalId == channel.Id.ToString() && ci.Protocol == PROTOCOL);
        if (c == null)
        {
            c = new Channel();
            _db.Channels.Add(c);
        }

        c.DisplayName = channel.Name;
        c.ExternalId = channel.Id.ToString();
        c.IsDM = false;
        c.Messages = c.Messages ?? new List<Message>();
        c.Protocol = protocolAsChannel.Protocol;
        c.ParentChannel = protocolAsChannel;
        c.SubChannels = c.SubChannels ?? new List<Channel>();
        c.Permissions = c.Permissions ?? new PermissionSettings();
        c.Permissions.MaxAttachmentBytes = channel.MaxUploadLimit;

        c.SendMessage = (t) => { throw new InvalidOperationException($"channel {channel.Name} is guild; cannot accept text"); };
        c.SendFile = (f, t) => { throw new InvalidOperationException($"channel {channel.Name} is guild; send file"); };
        return c;
    }
    internal Account UpsertAccount(IUser user)
    {
        var hadToAdd = false;
        var acc = _db.Accounts.FirstOrDefault(ui => ui.ExternalId == user.Id.ToString() && ui.Protocol == PROTOCOL);
        if (acc == null)
        {
            acc = new Account();
            _db.Accounts.Add(acc);
            hadToAdd = true;
        }
        acc.Username = user.Username;
        acc.ExternalId = user.Id.ToString();
        acc.IsBot = user.IsBot || user.IsWebhook;
        acc.Protocol = PROTOCOL;

        if(hadToAdd)
        {
            acc.IsUser = _db.Users.FirstOrDefault(u => u.Accounts.Any(a => a.ExternalId == acc.ExternalId && a.Protocol == acc.Protocol));
            if(acc.IsUser == null)
            {
                acc.IsUser = new User() { Accounts = new List<Account>() { acc } };
                _db.Users.Add(acc.IsUser);
            }
        }
        return acc;
    }

    private Task attemptReact(IUserMessage msg, string e)
    {
        var c = _db.Channels.FirstOrDefault(c => c.ExternalId == msg.Channel.Id.ToString());
        //var preferredEmote = c.EmoteOverrides?[e] ?? e; //TODO: emote overrides
        var preferredEmote = e;
        Emoji emoji;
        if (Emoji.TryParse(preferredEmote, out emoji))
        {
            return msg.AddReactionAsync(emoji);
        }
        Emote emote;
        if (!Emote.TryParse(preferredEmote, out emote))
        {
            if (preferredEmote == e)
                Console.Error.WriteLine($"never heard of emote {e}");
            return Task.CompletedTask;
        }

        return msg.AddReactionAsync(emote);
    }

}