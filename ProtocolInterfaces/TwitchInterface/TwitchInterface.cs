using RestSharp;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using vassago.Behavior;
using vassago.Models;

namespace vassago.TwitchInterface;

internal class unifiedTwitchMessage
{
    public unifiedTwitchMessage(ChatMessage chatMessage) { }
}

public class TwitchInterface
{
    internal const string PROTOCOL = "twitch";
    private static SemaphoreSlim channelSetupSemaphpore = new SemaphoreSlim(1, 1);
    private Channel protocolAsChannel;
    private Account selfAccountInProtocol;
    TwitchClient client;

    private async Task SetupTwitchChannel()
    {
        await channelSetupSemaphpore.WaitAsync();

        try
        {
            protocolAsChannel = Rememberer.SearchChannel(c => c.ParentChannel == null && c.Protocol == PROTOCOL);
            if (protocolAsChannel == null)
            {
                protocolAsChannel = new Channel()
                {
                    DisplayName = "twitch (itself)",
                    MeannessFilterLevel = Enumerations.MeannessFilterLevel.Medium,
                    LewdnessFilterLevel = Enumerations.LewdnessFilterLevel.G,
                    MaxTextChars = 500,
                    MaxAttachmentBytes = 0,
                    LinksAllowed = false,
                    ReactionsPossible = false,
                    ExternalId = null,
                    Protocol = PROTOCOL,
                    SubChannels = []
                };
                protocolAsChannel.DisplayName = "twitch (itself)";
                protocolAsChannel.SendMessage = (t) => { throw new InvalidOperationException($"twitch itself cannot accept text"); };
                protocolAsChannel.SendFile = (f, t) => { throw new InvalidOperationException($"twitch itself cannot send file"); };
                protocolAsChannel = Rememberer.RememberChannel(protocolAsChannel);
                Console.WriteLine($"protocol as channle added; {protocolAsChannel}");
            }
            else
            {
                Console.WriteLine($"twitch, channel with id {protocolAsChannel.Id}, already exists");
            }
            //protocolAsChan
        }
        finally
        {
            channelSetupSemaphpore.Release();
        }
    }

    ///<param name="oauth">https://www.twitchapps.com/tmi/</param>
    public async Task Init(TwitchConfig tc)
    {
        await SetupTwitchChannel();

        WebSocketClient customClient = new WebSocketClient(new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        }
        );
        client = new TwitchClient(customClient);
        client.Initialize(new ConnectionCredentials(tc.username, tc.oauth, capabilities: new Capabilities()));

        client.OnLog += Client_OnLog;
        client.OnJoinedChannel += Client_OnJoinedChannel;
        client.OnMessageReceived += Client_OnMessageReceivedAsync;
        client.OnWhisperReceived += Client_OnWhisperReceivedAsync;
        client.OnConnected += Client_OnConnected;

        client.Connect();
        Console.WriteLine("twitch client 1 connected");
    }

    private async void Client_OnWhisperReceivedAsync(object sender, OnWhisperReceivedArgs e)
    {
        //data received
        Console.WriteLine($"whisper#{e.WhisperMessage.Username}[{DateTime.Now}][{e.WhisperMessage.DisplayName} [id={e.WhisperMessage.Username}]][msg id: {e.WhisperMessage.MessageId}] {e.WhisperMessage.Message}");

        //translate to internal, upsert
        var m = UpsertMessage(e.WhisperMessage);
        m.Reply = (t) => { return Task.Run(() => { client.SendWhisper(e.WhisperMessage.Username, t); }); };
        m.Channel.ChannelType = vassago.Models.Enumerations.ChannelType.DM;
        //act on
        await Behaver.Instance.ActOn(m);
        m.ActedOn = true;
        //TODO: remember it again?
    }

    private async void Client_OnMessageReceivedAsync(object sender, OnMessageReceivedArgs e)
    {
        //data eived
        Console.WriteLine($"#{e.ChatMessage.Channel}[{DateTime.Now}][{e.ChatMessage.DisplayName} [id={e.ChatMessage.Username}]][msg id: {e.ChatMessage.Id}] {e.ChatMessage.Message}");

        //translate to internal, upsert
        var m = UpsertMessage(e.ChatMessage);
        m.Reply = (t) => { return Task.Run(() => { client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, t); }); };
        m.Channel.ChannelType = vassago.Models.Enumerations.ChannelType.Normal;
        //act on
        await Behaver.Instance.ActOn(m);
        m.ActedOn = true;
        //TODO: remember again?
    }

    private void Client_OnConnected(object sender, OnConnectedArgs e)
    {
        Console.WriteLine($"twitch marking selfaccount as seeninchannel {protocolAsChannel.Id}");
        selfAccountInProtocol = UpsertAccount(e.BotUsername, protocolAsChannel);
        selfAccountInProtocol.DisplayName = e.BotUsername;
        Behaver.Instance.MarkSelf(selfAccountInProtocol);

        Console.WriteLine($"Connected to {e.AutoJoinChannel}");
    }

    private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
    {
        client.SendMessage(e.Channel, "beep boop");
    }

    private void Client_OnLog(object sender, OnLogArgs e)
    {
        Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
    }

    private Account UpsertAccount(string username, Channel inChannel)
    {
        //Console.WriteLine($"upserting twitch account. username: {username}. inChannel: {inChannel?.Id}");
        var acc = Rememberer.SearchAccount(ui => ui.ExternalId == username && ui.SeenInChannel.ExternalId == inChannel.ExternalId);
        // Console.WriteLine($"upserting twitch account, retrieved {acc?.Id}.");
        if (acc != null)
        {
            Console.WriteLine($"acc's usser: {acc.IsUser?.Id}");
        }
        acc ??= new Account()
        {
            IsUser = Rememberer.SearchUser(
                u => u.Accounts.Any(a => a.ExternalId == username && a.Protocol == PROTOCOL))
                ?? new vassago.Models.User()
        };

        acc.Username = username;
        acc.ExternalId = username;
        //acc.IsBot = false? there is a way to tell, but you have to go back through the API
        acc.Protocol = PROTOCOL;
        acc.SeenInChannel = inChannel;

        // Console.WriteLine($"we asked rememberer to search for acc's user. {acc.IsUser?.Id}");
        // if (acc.IsUser != null)
        // {
        //     Console.WriteLine($"user has record of {acc.IsUser.Accounts?.Count ?? 0} accounts");
        // }
        acc.IsUser ??= new vassago.Models.User() { Accounts = [acc] };
        // if (inChannel.Users?.Count > 0)
        // {
        //     Console.WriteLine($"channel has {inChannel.Users.Count} accounts");
        // }
        Rememberer.RememberAccount(acc);
        inChannel.Users ??= [];
        if (!inChannel.Users.Contains(acc))
        {
            inChannel.Users.Add(acc);
            Rememberer.RememberChannel(inChannel);
        }
        return acc;
    }

    private Channel UpsertChannel(string channelName)
    {
        Channel c = Rememberer.SearchChannel(ci => ci.ExternalId == channelName
                                             && ci.Protocol == PROTOCOL);
        if (c == null)
        {
            // Console.WriteLine($"couldn't find channel under protocol {PROTOCOL} with externalId {channelName}");
            c = new Channel()
            {
                Users = []
            };
        }

        c.DisplayName = channelName;
        c.ExternalId = channelName;
        c.ChannelType = vassago.Models.Enumerations.ChannelType.Normal;
        c.Messages ??= [];
        c.Protocol = PROTOCOL;
        c.ParentChannel = protocolAsChannel;
        c.SubChannels = c.SubChannels ?? new List<Channel>();
        c.SendMessage = (t) => { return Task.Run(() => { client.SendMessage(channelName, t); }); };
        c.SendFile = (f, t) => { throw new InvalidOperationException($"twitch cannot send files"); };
        c = Rememberer.RememberChannel(c);

        var selfAccountInChannel = c.Users?.FirstOrDefault(a => a.ExternalId == selfAccountInProtocol.ExternalId);
        if (selfAccountInChannel == null)
        {
            selfAccountInChannel = UpsertAccount(selfAccountInProtocol.Username, c);
        }

        return c;
    }
    private Channel UpsertDMChannel(string whisperWith)
    {
        Channel c = Rememberer.SearchChannel(ci => ci.ExternalId == $"w_{whisperWith}"
                                                     && ci.Protocol == PROTOCOL);
        if (c == null)
        {
            // Console.WriteLine($"couldn't find channel under protocol {PROTOCOL}, whisper with {whisperWith}");
            c = new Channel()
            {
                Users = []
            };
        }

        c.DisplayName = $"Whisper: {whisperWith}";
        c.ExternalId = $"w_{whisperWith}";
        c.ChannelType = vassago.Models.Enumerations.ChannelType.DM;
        c.Messages ??= [];
        c.Protocol = PROTOCOL;
        c.ParentChannel = protocolAsChannel;
        c.SubChannels = c.SubChannels ?? new List<Channel>();
        c.SendMessage = (t) =>
        {
            return Task.Run(() =>
            {
                try
                {

                    client.SendWhisper(whisperWith, t);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
            });
        };
        c.SendFile = (f, t) => { throw new InvalidOperationException($"twitch cannot send files"); };
        c = Rememberer.RememberChannel(c);

        var selfAccountInChannel = c.Users.FirstOrDefault(a => a.ExternalId == selfAccountInProtocol.ExternalId);
        if (selfAccountInChannel == null)
        {
            selfAccountInChannel = UpsertAccount(selfAccountInChannel.Username, c);
        }

        return c;
    }

    //n.b., I see you future adam. "we should unify these, they're redundant".
    //ah, but that's the trick, they aren't! twitchlib has a common base class, but
    //none of the features we care about are on it!
    private Message UpsertMessage(ChatMessage chatMessage)
    {
        var m = Rememberer.SearchMessage(mi => mi.ExternalId == chatMessage.Id && mi.Protocol == PROTOCOL)
            ?? new()
            {
                Protocol = PROTOCOL,
                Timestamp = (DateTimeOffset)DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };
        m.Content = chatMessage.Message;
        m.ExternalId = chatMessage.Id;
        m.Channel = UpsertChannel(chatMessage.Channel);
        m.Author = UpsertAccount(chatMessage.Username, m.Channel);
        m.MentionsMe = Regex.IsMatch(m.Content?.ToLower(), $"@\\b{selfAccountInProtocol.Username.ToLower()}\\b");
        m.Reply = (t) => { return Task.Run(() => { client.SendReply(chatMessage.Channel, chatMessage.Id, t); }); };
        m.React = (e) => { throw new InvalidOperationException($"twitch cannot react"); };
        Rememberer.RememberMessage(m);
        return m;
    }
    //n.b., I see you future adam. "we should unify these, they're redundant".
    //ah, but that's the trick, they aren't! twitchlib has a common base class, but
    //none of the features we care about are on it!
    private Message UpsertMessage(WhisperMessage whisperMessage)
    {
        //WhisperMessage.Id corresponds to chatMessage.Id. \*eye twitch*
        var m = Rememberer.SearchMessage(mi => mi.ExternalId == whisperMessage.MessageId && mi.Protocol == PROTOCOL)
            ?? new()
            {
                Protocol = PROTOCOL,
                Timestamp = (DateTimeOffset)DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };
        m.Content = whisperMessage.Message;
        m.ExternalId = whisperMessage.MessageId;
        m.Channel = UpsertDMChannel(whisperMessage.Username);
        m.Author = UpsertAccount(whisperMessage.Username, m.Channel);
        m.MentionsMe = Regex.IsMatch(m.Content?.ToLower(), $"@\\b{selfAccountInProtocol.Username.ToLower()}\\b");
        m.Reply = (t) => { return Task.Run(() => { client.SendWhisper(whisperMessage.Username, t); }); };
        m.React = (e) => { throw new InvalidOperationException($"twitch cannot react"); };
        Rememberer.RememberMessage(m);
        return m;
    }

    public string AttemptJoin(string channelTarget)
    {
        client.JoinChannel(channelTarget);
        return $"attempt join {channelTarget} - o7";
    }

    internal void AttemptLeave(string channelTarget)
    {
        client.SendMessage(channelTarget, "o7");
        client.LeaveChannel(channelTarget);
    }
}
