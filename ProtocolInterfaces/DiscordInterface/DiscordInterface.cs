//https://discord.com/oauth2/authorize?client_id=913003037348491264&permissions=274877942784&scope=bot%20messages.read
namespace vassago.ProtocolInterfaces;

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
using Newtonsoft.Json;
using static vassago.Models.Enumerations;


//data received
//translate data to internal type
//store
//ship off to behaver

public class DiscordInterface : ProtocolInterface
{
    public static new string Protocol { get => "discord"; }
    internal DiscordSocketClient client;
    private bool eventsSignedUp = false;
    private static readonly SemaphoreSlim discordChannelSetup = new(1, 1);
    private Channel protocolAsChannel;
    public override Channel SelfChannel { get => protocolAsChannel; }

    private static ProtocolDiscord confEntity;
    public override ProtocolConfiguration ConfigurationEntity { get => confEntity; }

    public async Task Init(ProtocolDiscord cfg)
    {
        confEntity = cfg;
        var token = confEntity.token;
        Console.WriteLine($"going to validate token {token}");
        Discord.TokenUtils.ValidateToken(TokenType.Bot, token);//throws an exception if invalid
        await SetupDiscordChannel();
        client = new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All });

        client.Log += (msg) =>
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        };
        client.Connected += this.SelfConnected;
        client.Disconnected += this.ClientDisconnected;
        client.Ready += this.ClientReady;

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
    }
    public override async Task<int> Die()
    {
        await client.StopAsync();
        client = null;
        return 200;
    }
    public override async Task<int> UpdateConfiguration(ProtocolConfiguration incomingCfg)
    {
        var newConfEntity = incomingCfg as ProtocolDiscord;
        if (newConfEntity != null)
        {
            Console.WriteLine("Discord Interface was able to cast incoming configuration to a discord configuration");
            if (newConfEntity.token != confEntity.token)
            {
                await client.StopAsync();
                try
                {
                    Discord.TokenUtils.ValidateToken(TokenType.Bot, newConfEntity.token);//throws an exception if invalid. and because microsoft, who fucking knows where that goes.
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Token invalid. {JsonConvert.SerializeObject(e)}");
                    client = null;
                    return 400;
                }
                confEntity = newConfEntity;
                await client.LoginAsync(TokenType.Bot, confEntity.token);
                await client.StartAsync();
                return 200;
            }
            return 200;
        }
        else
        {
            Console.Error.WriteLine("update configuration for discord interface handling {confEntity.Id} given invalid configuration:");
            Console.Error.WriteLine(JsonConvert.SerializeObject(incomingCfg));
            Die();
            return 422;
        }
    }
    private async Task ClientDisconnected(Exception e)
    {
        Console.WriteLine("client disconnected!");
        Console.WriteLine(e?.Message);
    }

    private async Task SetupDiscordChannel()
    {
        await discordChannelSetup.WaitAsync();

        try
        {
            protocolAsChannel = r.SearchChannel(c => c.ParentChannel == null && c.Protocol == Protocol);
            if (protocolAsChannel == null)
            {
                protocolAsChannel = new Channel()
                {
                    DisplayName = "discord (itself)",
                    MeannessFilterLevel = Enumerations.MeannessFilterLevel.Strict,
                    LewdnessFilterLevel = Enumerations.LewdnessFilterLevel.Moderate,
                    MaxTextChars = 2000,
                    MaxAttachmentBytes = 25 * 1024 * 1024, //allegedly it's 25, but I worry it's not actually.
                    LinksAllowed = true,
                    ReactionsPossible = true,
                    ExternalId = null,
                    Protocol = Protocol,
                    SubChannels = []
                };
                base.basedot_ChannelJoined(protocolAsChannel);
            }
            else
            {
                Console.WriteLine($"discord, channel with id {protocolAsChannel.Id}, already exists");
            }
            protocolAsChannel.DisplayName = "discord (itself)";
            protocolAsChannel = r.RememberChannel(protocolAsChannel);
            Console.WriteLine($"protocol as channel addeed; {protocolAsChannel}");
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

            client.MessageReceived += DiscordMessageReceived;
            // _client.MessageUpdated +=
            client.UserJoined += UserJoined;
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
        await discordChannelSetup.WaitAsync();

        try
        {
            var selfAccount = UpsertAccount(client.CurrentUser, protocolAsChannel);
            selfAccount.DisplayName = client.CurrentUser.Username;
            Behaver.Instance.MarkSelf(selfAccount);
        }
        finally
        {
            discordChannelSetup.Release();
        }
    }

    private async Task DiscordMessageReceived(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage)
        {
            Console.WriteLine($"{messageParam.Content}, but not a user message");
            return;
        }
        var suMessage = messageParam as SocketUserMessage;

        Console.WriteLine($"#{suMessage.Channel}[{DateTime.Now}][{suMessage.Author.Username} [id={suMessage.Author.Id}]][msg id: {suMessage.Id}] {suMessage.Content}");

        var m = UpsertMessage(suMessage);

        await Behaver.Instance.ActOn(m);
        m.ActedOn = true; // for its own ruposess it might act on it later, but either way, fuck it, we checked.
                          // ...but we don't save?
                          // TODO: do we actually need this?

        base.basedot_MessageReceived(m);
    }

    private async Task UserJoined(SocketGuildUser arg)
    {
        Console.WriteLine($"discord interface sees a user has joined a guild.");
        var guild = UpsertChannel(arg.Guild);
        var defaultChannel = UpsertChannel(arg.Guild.DefaultChannel);
        defaultChannel.ParentChannel = guild;
        var u = UpsertAccount(arg, guild);
        u.DisplayName = arg.DisplayName;
        base.basedot_AccountMet(u);
    }

    internal static vassago.Models.Attachment UpsertAttachment(IAttachment dAttachment)
    {
        var a = r.SearchAttachment(ai => ai.ExternalId == dAttachment.Id)
            ?? new vassago.Models.Attachment();

        a.ContentType = dAttachment.ContentType;
        a.Description = dAttachment.Description;
        a.Filename = dAttachment.Filename;
        a.Size = dAttachment.Size;
        a.Source = new Uri(dAttachment.Url);
        r.RememberAttachment(a);
        return a;
    }
    internal Message UpsertMessage(IUserMessage dMessage)
    {
        var m = r.SearchMessage(mi => mi.ExternalId == dMessage.Id.ToString() && mi.Protocol == Protocol)
            ?? new()
            {
                Protocol = Protocol
            };

        if ((dMessage as SocketMessage)?.MentionedUsers?.FirstOrDefault(muid => muid.Id == client.CurrentUser.Id) != null)
        {
            var mentionOfMe = "<@" + client.CurrentUser.Id + ">";
            m.MentionsMe = true;
        }
        if (dMessage.Attachments?.Count > 0)
        {
            m.Attachments = [];
            foreach (var da in dMessage.Attachments)
            {
                m.Attachments.Add(UpsertAttachment(da));
            }
        }
        m.Content = dMessage.Content;
        m.ExternalId = dMessage.Id.ToString();
        m.Timestamp = dMessage.EditedTimestamp ?? dMessage.CreatedAt;
        m.Channel = UpsertChannel(dMessage.Channel);
        m.Author = UpsertAccount(dMessage.Author, m.Channel);
        if (dMessage.Channel is IGuildChannel)
        {
            m.Author.DisplayName = (dMessage.Author as IGuildUser).DisplayName;//discord forgot how display names work.
        }
        m.MentionsMe = (dMessage.Author.Id != client.CurrentUser.Id
            && (dMessage.MentionedUserIds?.FirstOrDefault(muid => muid == client.CurrentUser.Id) > 0));

        r.RememberMessage(m);
        Console.WriteLine($"received message; author: {m.Author.DisplayName}, {m.Author.Id}. messageid:{m.Id}");
        return m;
    }
    internal Channel UpsertChannel(IMessageChannel channel)
    {
        var channelDirtiness = DataDirtiness.Untouched;
        Channel c = r.SearchChannel(ci => ci.ExternalId == channel.Id.ToString() && ci.Protocol == Protocol);
        if (c == null)
        {
            channelDirtiness = DataDirtiness.New;
            Console.WriteLine($"couldn't find channel under protocol {Protocol} with externalId {channel.Id.ToString()}");
            c = new Channel()
            {
                Users = []
            };
        }

        if (channelDirtiness == DataDirtiness.Untouched && c.ExternalId != channel.Id.ToString()) channelDirtiness = DataDirtiness.Dirty;
        c.ExternalId = channel.Id.ToString();
        if (channelDirtiness == DataDirtiness.Untouched && c.ChannelType != ((channel is IPrivateChannel) ? vassago.Models.Enumerations.ChannelType.DM : vassago.Models.Enumerations.ChannelType.Normal)) channelDirtiness = DataDirtiness.Dirty;
        c.ChannelType = (channel is IPrivateChannel) ? vassago.Models.Enumerations.ChannelType.DM : vassago.Models.Enumerations.ChannelType.Normal;
        c.Messages ??= [];
        c.Protocol = Protocol;
        if (channel is IGuildChannel)
        {
            Console.WriteLine($"{channel.Name} is a guild channel. So i'm going to upsert the guild, {(channel as IGuildChannel).Guild}");
            c.ParentChannel = UpsertChannel((channel as IGuildChannel).Guild);
        }
        else if (channel is IPrivateChannel)
        {
            c.ParentChannel = protocolAsChannel;
            Console.WriteLine("i'm a private channel so I'm setting my parent channel to the protocol as channel");
        }
        else
        {
            c.ParentChannel = protocolAsChannel;
            Console.Error.WriteLine($"trying to upsert channel {channel.Id}/{channel.Name}, but it's neither guildchannel nor private channel. shrug.jpg");
        }

        Console.WriteLine($"upsertion of channel {c.DisplayName}, it's type {c.ChannelType}");
        switch (c.ChannelType)
        {
            case vassago.Models.Enumerations.ChannelType.DM:
                var asPriv = (channel as IPrivateChannel);
                var sender = asPriv?.Recipients?.FirstOrDefault(u => u.Id != client.CurrentUser.Id); // why yes, there's a list of recipients, and it's the sender.
                if (sender != null)
                {
                    c.DisplayName = "DM: " + sender.Username;
                }
                else
                {
                    //I sent it, so I don't know the recipient's name.
                }
                break;
            default:
                if (channelDirtiness == DataDirtiness.Untouched && c.DisplayName != channel.Name) channelDirtiness = DataDirtiness.Dirty;
                c.DisplayName = channel.Name;
                break;
        }

        Channel parentChannel = null;
        if (channel is IGuildChannel)
        {
            parentChannel = r.SearchChannel(c => c.ExternalId == (channel as IGuildChannel).Guild.Id.ToString() && c.Protocol == Protocol);
            if (parentChannel is null)
            {
                Console.Error.WriteLine("why am I still null?");
            }
        }
        else if (channel is IPrivateChannel)
        {
            parentChannel = protocolAsChannel;
        }
        else
        {
            parentChannel = protocolAsChannel;
            Console.Error.WriteLine($"trying to upsert channel {channel.Id}/{channel.Name}, but it's neither guildchannel nor private channel. shrug.jpg");
        }
        parentChannel.SubChannels ??= [];
        if (!parentChannel.SubChannels.Contains(c))
        {
            parentChannel.SubChannels.Add(c);
        }

        c = r.RememberChannel(c);

        var selfAccountInChannel = c.Users?.FirstOrDefault(a => a.ExternalId == client.CurrentUser.Id.ToString());
        if (selfAccountInChannel == null)
        {
            selfAccountInChannel = UpsertAccount(client.CurrentUser, c);
        }
        switch (channelDirtiness) {
            case DataDirtiness.Dirty:
                base.basedot_ChannelUpdated(c);
                break;
            case DataDirtiness.New:
                base.basedot_ChannelJoined(c);
                break;
        }

        return c;
    }
    internal Channel UpsertChannel(IGuild channel)
    {
        var channelDirtiness = DataDirtiness.Untouched;
        Channel c = r.SearchChannel(ci => ci.ExternalId == channel.Id.ToString() && ci.Protocol == Protocol);
        if (c == null)
        {
            Console.WriteLine($"couldn't find channel under protocol {Protocol} with externalId {channel.Id.ToString()}");
            c = new Channel();
            channelDirtiness = DataDirtiness.New;
        }

        if (channelDirtiness == DataDirtiness.Untouched && c.DisplayName != channel.Name) channelDirtiness = DataDirtiness.Dirty;
        c.DisplayName = channel.Name;
        c.ExternalId = channel.Id.ToString();
        c.ChannelType = vassago.Models.Enumerations.ChannelType.OU;
        c.Messages ??= [];
        c.Protocol = protocolAsChannel.Protocol;
        c.ParentChannel = protocolAsChannel;
        c.SubChannels ??= [];
        c.MaxAttachmentBytes = channel.MaxUploadLimit;
        switch (channelDirtiness) {
            case DataDirtiness.Dirty:
                base.basedot_ChannelUpdated(c);
                break;
            case DataDirtiness.New:
                base.basedot_ChannelJoined(c);
                break;
        }
        return r.RememberChannel(c);
    }
    internal Account UpsertAccount(IUser discordUser, Channel inChannel)
    {
        var accountDirtiness = DataDirtiness.Untouched;
        var acc = r.SearchAccount(ui => ui.ExternalId == discordUser.Id.ToString() && ui.SeenInChannel.Id == inChannel.Id);
        Console.WriteLine($"upserting account, retrieved {acc?.Id}.");
        if (acc != null)
        {
            Console.WriteLine($"acc's user: {acc.IsUser?.Id}");
        }
        else
        {
            acc = new Account()
            {
                IsUser = r.SearchUser(u => u.Accounts.Any(a => a.ExternalId == discordUser.Id.ToString() && a.Protocol == Protocol))
                ?? new User()
            };
            accountDirtiness = DataDirtiness.New;
        }

        if (accountDirtiness == DataDirtiness.Untouched && acc.Username != discordUser.Username) accountDirtiness= DataDirtiness.Dirty;
        acc.Username = discordUser.Username;
        acc.ExternalId = discordUser.Id.ToString();
        acc.IsBot = discordUser.IsBot || discordUser.IsWebhook;
        acc.Protocol = Protocol;
        acc.SeenInChannel = inChannel;

        Console.WriteLine($"we asked rememberer to search for acc's user. {acc.IsUser?.Id}");
        if (acc.IsUser != null)
        {
            Console.WriteLine($"user has record of {acc.IsUser.Accounts?.Count ?? 0} accounts");
        }
        acc.IsUser ??= new User() { Accounts = [acc] };
        if (inChannel.Users?.Count > 0)
        {
            Console.WriteLine($"channel has {inChannel.Users.Count} accounts");
        }
        r.RememberAccount(acc);
        inChannel.Users ??= [];
        if (!inChannel.Users.Contains(acc))
        {
            inChannel.Users.Add(acc);
            r.RememberChannel(inChannel);
        }
        switch (accountDirtiness) {
            case DataDirtiness.Dirty:
                base.basedot_AccountUpdated(acc);
                break;
            case DataDirtiness.New:
                base.basedot_AccountMet(acc);
                break;
        }
        return acc;
    }

    private static async Task<int> AttemptReact(IUserMessage msg, string e)
    {
        Console.WriteLine("discord attempting to react");
        var c = r.SearchChannel(c => c.ExternalId == msg.Channel.Id.ToString());// db.Channels.FirstOrDefault(c => c.ExternalId == msg.Channel.Id.ToString());
        //var preferredEmote = c.EmoteOverrides?[e] ?? e; //TODO: emote overrides
        var preferredEmote = e;
        if (Emoji.TryParse(preferredEmote, out Emoji emoji))
        {
            msg.AddReactionAsync(emoji);
            return 200;
        }
        if (!Emote.TryParse(preferredEmote, out Emote emote))
        {
            if (preferredEmote == e)
                Console.Error.WriteLine($"never heard of emote {e}");
            return 405;
        }

        msg.AddReactionAsync(emote);
        return 200;
    }

    private static string TruncateText(string msg, uint? chars)
    {
        chars ??= 500;
        if (msg?.Length > chars)
        {
            return msg.Substring(0, (int)chars - 2) + "✂";
        }
        else
        {
            return msg;
        }
    }
    public override async Task<int> SendMessage(Channel channel, string text)
    {
        var dcCh = await client.GetChannelAsync(ulong.Parse(channel.ExternalId));
        if (dcCh == null)
        {
            return 404;
        }

        if (dcCh is IMessageChannel msgChannel)
        {
            await msgChannel.SendMessageAsync(TruncateText(text, channel.MaxTextChars));
            return 200;
        }
        else
        {
            return 503;
        }
    }
    public override async Task<int> SendFile(Channel channel, string base64dData, string filename, string accompanyingText)
    {
        var dcCh = await client.GetChannelAsync(ulong.Parse(channel.ExternalId));
        if (dcCh == null)
        {
            return 404;
        }

        if (dcCh is IMessageChannel msgChannel)
        {
            using (var ms = new MemoryStream(Convert.FromBase64String(base64dData)))
            {
                await msgChannel.SendFileAsync(ms, filename, TruncateText(accompanyingText, channel.MaxTextChars));
            }
            return 200;
        }
        else
        {
            return 503;
        }
    }
    public override async Task<int> React(Message message, string reaction)
    {
        var dcCh = await client.GetChannelAsync(ulong.Parse(message.Channel.ExternalId));
        if (dcCh == null)
            return 404;

        if (dcCh is IMessageChannel msgChannel)
        {
            var dcMsg = await msgChannel.GetMessageAsync(ulong.Parse(message.ExternalId));
            if (dcMsg == null)
                return 404;

            return await AttemptReact(dcMsg as IUserMessage, reaction);
        }
        else
        {
            return 503;
        }
    }
    public override async Task<int> Reply(Message message, string text)
    {
        var dcCh = await client.GetChannelAsync(ulong.Parse(message.Channel.ExternalId));
        if (dcCh == null)
            return 404;

        if (dcCh is IMessageChannel msgChannel)
        {
            var dcMsg = await msgChannel.GetMessageAsync(ulong.Parse(message.ExternalId));
            if (dcMsg == null)
                return 404;

            (dcMsg as IUserMessage).ReplyAsync(TruncateText(text, message.Channel.MaxTextChars));
            return 200;
        }
        else
        {
            return 503;
        }
    }
    internal static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.CommandName)
        {
            case "freedomunits":
                try
                {
                    var amt = (double)(command.Data.Options.First(o => o.Name == "amount").Value);
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
}
