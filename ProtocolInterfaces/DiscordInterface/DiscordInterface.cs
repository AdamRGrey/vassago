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

namespace vassago.ProtocolInterfaces.DiscordInterface;

    //data received
    //translate data to internal type
    //store
    //ship off to behaver

public class DiscordInterface
{
    internal static string PROTOCOL { get => "discord"; }
    internal DiscordSocketClient client;
    private bool eventsSignedUp = false;
    private static readonly SemaphoreSlim discordChannelSetup = new(1, 1);
    private Channel protocolAsChannel;

    public async Task Init(string config)
    {
        var token = config;
        await SetupDiscordChannel();
        client = new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All });

        client.Log += (msg) =>
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        };
        client.Connected += () => Task.Run(SelfConnected);
        client.Ready += () => Task.Run(ClientReady);

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
    }

    private async Task SetupDiscordChannel()
    {
        await discordChannelSetup.WaitAsync();

        try
        {
            protocolAsChannel = Rememberer.SearchChannel(c => c.ParentChannel == null && c.Protocol == PROTOCOL);
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
                    Protocol = PROTOCOL,
                    SubChannels = []
                };
            }
            else
            {
                Console.WriteLine($"discord, channel with id {protocolAsChannel.Id}, already exists");
            }
            protocolAsChannel.DisplayName = "discord (itself)";
            protocolAsChannel.SendMessage = (t) => { throw new InvalidOperationException($"protocol isn't a real channel, cannot accept text"); };
            protocolAsChannel.SendFile = (f, t) => { throw new InvalidOperationException($"protocol isn't a real channel, cannot send file"); };
            protocolAsChannel = Rememberer.RememberChannel(protocolAsChannel);
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

            client.MessageReceived += MessageReceived;
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

    private async Task MessageReceived(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage)
        {
            Console.WriteLine($"{messageParam.Content}, but not a user message");
            return;
        }
        var suMessage = messageParam as SocketUserMessage;

        Console.WriteLine($"#{suMessage.Channel}[{DateTime.Now}][{suMessage.Author.Username} [id={suMessage.Author.Id}]][msg id: {suMessage.Id}] {suMessage.Content}");

        var m = UpsertMessage(suMessage);

        if (suMessage.MentionedUsers?.FirstOrDefault(muid => muid.Id == client.CurrentUser.Id) != null)
        {
            var mentionOfMe = "<@" + client.CurrentUser.Id + ">";
            m.MentionsMe = true;
        }
        await Behaver.Instance.ActOn(m);
        m.ActedOn = true; // for its own ruposess it might act on it later, but either way, fuck it, we checked.
                          // ...but we don't save?
    }

    private Task UserJoined(SocketGuildUser arg)
    {
        var guild = UpsertChannel(arg.Guild);
        var defaultChannel = UpsertChannel(arg.Guild.DefaultChannel);
        defaultChannel.ParentChannel = guild;
        var u = UpsertAccount(arg, guild);
        u.DisplayName = arg.DisplayName;
        return null;
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
    internal static vassago.Models.Attachment UpsertAttachment(IAttachment dAttachment)
    {
        var a = Rememberer.SearchAttachment(ai => ai.ExternalId == dAttachment.Id)
            ?? new vassago.Models.Attachment();

        a.ContentType = dAttachment.ContentType;
        a.Description = dAttachment.Description;
        a.Filename = dAttachment.Filename;
        a.Size = dAttachment.Size;
        a.Source = new Uri(dAttachment.Url);
        Rememberer.RememberAttachment(a);
        return a;
    }
    internal Message UpsertMessage(IUserMessage dMessage)
    {
        var m = Rememberer.SearchMessage(mi => mi.ExternalId == dMessage.Id.ToString() && mi.Protocol == PROTOCOL)
            ?? new()
            {
                Protocol = PROTOCOL
            };

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
        Console.WriteLine($"received message; author: {m.Author.DisplayName}, {m.Author.Id}");
        if (dMessage.Channel is IGuildChannel)
        {
            m.Author.DisplayName = (dMessage.Author as IGuildUser).DisplayName;//discord forgot how display names work.
        }
        m.MentionsMe = (dMessage.Author.Id != client.CurrentUser.Id
            && (dMessage.MentionedUserIds?.FirstOrDefault(muid => muid == client.CurrentUser.Id) > 0));

        m.Reply = (t) => { return dMessage.ReplyAsync(TruncateText(t, m.Channel.MaxTextChars)); };
        m.React = (e) => { return AttemptReact(dMessage, e); };
        Rememberer.RememberMessage(m);
        return m;
    }
    internal Channel UpsertChannel(IMessageChannel channel)
    {
        Channel c = Rememberer.SearchChannel(ci => ci.ExternalId == channel.Id.ToString() && ci.Protocol == PROTOCOL);
        if (c == null)
        {
            Console.WriteLine($"couldn't find channel under protocol {PROTOCOL} with externalId {channel.Id.ToString()}");
            c = new Channel()
            {
                Users = []
            };
        }

        c.ExternalId = channel.Id.ToString();
        c.ChannelType = (channel is IPrivateChannel) ? vassago.Models.Enumerations.ChannelType.DM : vassago.Models.Enumerations.ChannelType.Normal;
        c.Messages ??= [];
        c.Protocol = PROTOCOL;
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
                var asPriv =(channel as IPrivateChannel);
                var sender = asPriv?.Recipients?.FirstOrDefault(u => u.Id != client.CurrentUser.Id); // why yes, there's a list of recipients, and it's the sender.
                if(sender != null)
                {
                    c.DisplayName = "DM: " + sender.Username;
                }
                else
                {
                    //I sent it, so I don't know the recipient's name.
                }
                break;
            default:
                c.DisplayName = channel.Name;
                break;
        }

        Channel parentChannel = null;
        if (channel is IGuildChannel)
        {
            parentChannel = Rememberer.SearchChannel(c => c.ExternalId == (channel as IGuildChannel).Guild.Id.ToString() && c.Protocol == PROTOCOL);
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
        if(!parentChannel.SubChannels.Contains(c))
        {
            parentChannel.SubChannels.Add(c);
        }

        c.SendMessage = (t) => { return channel.SendMessageAsync(TruncateText(t, c.MaxTextChars));};
        c.SendFile = (f, t) => { return channel.SendFileAsync(f, t); };

        c = Rememberer.RememberChannel(c);

        //Console.WriteLine($"no one knows how to make good tooling. c.users.first, which needs client currentuser id tostring. c: {c}, c.Users {c.Users}, client: {client}, client.CurrentUser: {client.CurrentUser}, client.currentUser.Id: {client.CurrentUser.Id}");
        var selfAccountInChannel = c.Users?.FirstOrDefault(a => a.ExternalId == client.CurrentUser.Id.ToString());
        if(selfAccountInChannel == null)
        {
            selfAccountInChannel = UpsertAccount(client.CurrentUser, c);
        }

        return c;
    }
    internal Channel UpsertChannel(IGuild channel)
    {
        Channel c = Rememberer.SearchChannel(ci => ci.ExternalId == channel.Id.ToString() && ci.Protocol == PROTOCOL);
        if (c == null)
        {
            Console.WriteLine($"couldn't find channel under protocol {PROTOCOL} with externalId {channel.Id.ToString()}");
            c = new Channel();
        }

        c.DisplayName = channel.Name;
        c.ExternalId = channel.Id.ToString();
        c.ChannelType = vassago.Models.Enumerations.ChannelType.OU;
        c.Messages ??= [];
        c.Protocol = protocolAsChannel.Protocol;
        c.ParentChannel = protocolAsChannel;
        c.SubChannels ??= [];
        c.MaxAttachmentBytes = channel.MaxUploadLimit;

        c.SendMessage = (t) => { throw new InvalidOperationException($"channel {channel.Name} is guild; cannot accept text"); };
        c.SendFile = (f, t) => { throw new InvalidOperationException($"channel {channel.Name} is guild; send file"); };

        return Rememberer.RememberChannel(c);
    }
    internal static Account UpsertAccount(IUser discordUser, Channel inChannel)
    {
        var acc = Rememberer.SearchAccount(ui => ui.ExternalId == discordUser.Id.ToString() && ui.SeenInChannel.Id == inChannel.Id);
        Console.WriteLine($"upserting account, retrieved {acc?.Id}.");
        if (acc != null)
        {
            Console.WriteLine($"acc's user: {acc.IsUser?.Id}");
        }
        acc ??= new Account() {
            IsUser = Rememberer.SearchUser(u => u.Accounts.Any(a => a.ExternalId == discordUser.Id.ToString() && a.Protocol == PROTOCOL))
                ?? new User()
        };

        acc.Username = discordUser.Username;
        acc.ExternalId = discordUser.Id.ToString();
        acc.IsBot = discordUser.IsBot || discordUser.IsWebhook;
        acc.Protocol = PROTOCOL;
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
        Rememberer.RememberAccount(acc);
        inChannel.Users ??= [];
        if(!inChannel.Users.Contains(acc))
        {
            inChannel.Users.Add(acc);
            Rememberer.RememberChannel(inChannel);
        }
        return acc;
    }

    private static Task AttemptReact(IUserMessage msg, string e)
    {
        var c = Rememberer.SearchChannel(c => c.ExternalId == msg.Channel.Id.ToString());// db.Channels.FirstOrDefault(c => c.ExternalId == msg.Channel.Id.ToString());
        //var preferredEmote = c.EmoteOverrides?[e] ?? e; //TODO: emote overrides
        var preferredEmote = e;
        if (Emoji.TryParse(preferredEmote, out Emoji emoji))
        {
            return msg.AddReactionAsync(emoji);
        }
        if (!Emote.TryParse(preferredEmote, out Emote emote))
        {
            if (preferredEmote == e)
                Console.Error.WriteLine($"never heard of emote {e}");
            return Task.CompletedTask;
        }

        return msg.AddReactionAsync(emote);
    }

    private static string TruncateText(string msg, uint? chars)
    {
        chars ??= 500;
        if(msg?.Length > chars)
        {
            return msg.Substring(0, (int)chars-2) + "✂";
        }
        else
        {
            return msg;
        }
    }

}
