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

namespace vassago.DiscordInterface;

public class DiscordInterface
{
    internal const string PROTOCOL = "discord";
    internal DiscordSocketClient client;
    private bool eventsSignedUp = false;
    private ChattingContext _db;
    private static PermissionSettings defaultPermissions = new PermissionSettings()
    {
        MeannessFilterLevel = 1,
        LewdnessFilterLevel = 3,
        MaxTextChars = 2000,
        MaxAttachmentBytes = 8 * 1024 * 1024,
        LinksAllowed = true,
        ReactionsPossible = true
    };
    public DiscordInterface()
    {
        _db = Shared.dbContext;
    }

    public async Task Init(string token)
    {
        client = new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All });

        client.Log += (msg) =>
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        };

        client.Ready += () => Task.Run(() =>
        {
            if (!eventsSignedUp)
            {
                eventsSignedUp = true;
                Console.WriteLine("Bot is connected! going to sign up for message received and user joined in client ready");

                client.MessageReceived += MessageReceived;
                // _client.MessageUpdated +=
                // client.UserJoined += UserJoined;
                client.SlashCommandExecuted += SlashCommandHandler;
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

                SlashCommandsHelper.Register(client).GetAwaiter().GetResult();
            }
            else
            {
                Console.WriteLine("bot appears to be RE connected, so I'm not going to sign up twice");
            }
        });

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
    }
    
    #pragma warning disable 4014 //the "you're not awaiting this" warning. yeah I know, that's the beauty of an async method lol
    #pragma warning disable 1998 //the "it's async but you're not awaiting anything".
    private async Task MessageReceived(SocketMessage messageParam)
    #pragma warning restore 1998
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

        if ((suMessage.Author.Id != client.CurrentUser.Id))
        {
            if (await Behaver.Instance.ActOn(m))
            {
                m.ActedOn = true;
            }
        }
        _db.SaveChanges();
    }

    private void UserJoined(SocketGuildUser arg)
    {
        
            var guild = UpsertChannel(arg.Guild);
            var defaultChannel = UpsertChannel(arg.Guild.DefaultChannel);
            defaultChannel.ParentChannel = guild;
            var u = UpsertUser(arg);
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
        var addPlease = false;
        var a = _db.Attachments.FirstOrDefault(ai => ai.ExternalId == dAttachment.Id);
        if (a == null)
        {
            addPlease = true;
            a = new vassago.Models.Attachment();
        }
        a.ContentType = dAttachment.ContentType;
        a.Description = dAttachment.Description;
        a.Filename = dAttachment.Filename;
        a.Size = dAttachment.Size;
        a.Source = new Uri(dAttachment.Url);

        if (addPlease)
        {
            _db.Attachments.Add(a);
        }
        return a;
    }
    internal Message UpsertMessage(IUserMessage dMessage)
    {
        var addPlease = false;
        var m = _db.Messages.FirstOrDefault(mi => mi.ExternalId == dMessage.Id);
        if (m == null)
        {
            addPlease = true;
            m = new Message();
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
        m.Author = UpsertUser(dMessage.Author);
        m.Channel = UpsertChannel(dMessage.Channel);
        m.Content = dMessage.Content;
        m.ExternalId = dMessage.Id;
        m.Timestamp = dMessage.EditedTimestamp ?? dMessage.CreatedAt;

        if (dMessage.MentionedUserIds?.FirstOrDefault(muid => muid == client.CurrentUser.Id) != null)
        {
            m.MentionsMe = true;
        }
        if (addPlease)
        {
            _db.Messages.Add(m);
        }

        m.Reply = (t) => { return dMessage.ReplyAsync(t); };
        m.React = (e) => { return dMessage.AddReactionAsync(Emote.Parse(e)); };
        return m;
    }
    internal Channel UpsertChannel(IMessageChannel channel)
    {
        var addPlease = false;
        Channel c = _db.Channels.FirstOrDefault(ci => ci.ExternalId == channel.Id);
        if (c == null)
        {
            addPlease = true;
            c = new Channel();
        }

        c.DisplayName = channel.Name;
        c.ExternalId = channel.Id;
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
            c.ParentChannel = null;
        }
        else
        {
            c.ParentChannel = null;
            Console.Error.WriteLine($"trying to upsert channel {channel.Id}/{channel.Name}, but it's neither guildchannel nor private channel. shrug.jpg");
        }
        c.SubChannels = c.SubChannels ?? new List<Channel>();
        if (addPlease)
        {
            _db.Channels.Add(c);
        }

        c.SendMessage = (t) => { return channel.SendMessageAsync(t); };
        c.SendFile = (f, t) => { return channel.SendFileAsync(f, t); };
        return c;
    }
    internal Channel UpsertChannel(IGuild channel)
    {
        var addPlease = false;
        Channel c = _db.Channels.FirstOrDefault(ci => ci.ExternalId == channel.Id);
        if (c == null)
        {
            addPlease = true;
            c = new Channel();
        }

        c.DisplayName = channel.Name;
        c.ExternalId = channel.Id;
        c.IsDM = false;
        c.Messages = c.Messages ?? new List<Message>();
        c.Protocol = PROTOCOL;
        c.ParentChannel = null;
        c.SubChannels = c.SubChannels ?? new List<Channel>();
        if (addPlease)
        {
            _db.Channels.Add(c);
        }

        c.SendMessage = (t) => { throw new InvalidOperationException($"channel {channel.Name} is guild; cannot accept text"); };
        c.SendFile = (f, t) => { throw new InvalidOperationException($"channel {channel.Name} is guild; send file"); };
        return c;
    }
    internal User UpsertUser(IUser user)
    {
        var addPlease = false;
        var u = _db.Users.FirstOrDefault(ui => ui.ExternalId == user.Id);
        if (u == null)
        {
            addPlease = true;
            u = new User();
        }
        u.Username = user.Username;
        u.ExternalId = user.Id;
        u.IsBot = user.IsBot || user.IsWebhook;
        u.Protocol = PROTOCOL;
        if (addPlease)
        {
            _db.Users.Add(u);
        }

        return u;
    }
    internal async void BackfillChannelInfo(Channel channel)
    {
        //TODO: some sort of "when you get around to it" task queue

        //c.Messages = await channel.GetMessagesAsync(); //TODO: this
        //c.OtherUsers = c.OtherUsers ?? new List<User>();
        //c.OtherUsers = await channel.GetUsersAsync(); 
        var dChannel = client.GetChannel(channel.ExternalId.Value);
        if(dChannel is IGuild)
        {
            var guild = channel as IGuild;
        }
        else if(dChannel is IGuildChannel)
        {
            var gc = dChannel as IGuildChannel;
        }
        else if (dChannel is IPrivateChannel)
        {
            var dm = dChannel as IPrivateChannel;
        }
    }
}