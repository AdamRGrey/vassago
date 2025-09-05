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
using vassago.ProtocolInterfaces;
using Newtonsoft.Json;

namespace vassago.ProtocolInterfaces;

public class TwitchInterface : ProtocolInterface
{
    public static new string Protocol { get => "twitch"; }
    private static SemaphoreSlim channelSetupSemaphpore = new SemaphoreSlim(1, 1);
    private Channel protocolAsChannel;
    public override Channel SelfChannel { get => protocolAsChannel;}
    private Account selfAccountInProtocol;
    TwitchClient client;

    private static ProtocolTwitch confEntity;
    public override ProtocolConfiguration ConfigurationEntity { get => confEntity; }


    private async Task SetupTwitchChannel()
    {
        await channelSetupSemaphpore.WaitAsync();

        try
        {
            protocolAsChannel = r.SearchChannel(c => c.ParentChannel == null && c.Protocol == Protocol);
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
                    Protocol = Protocol,
                    SubChannels = []
                };
                protocolAsChannel.DisplayName = "twitch (itself)";
                protocolAsChannel = r.RememberChannel(protocolAsChannel);
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
    public async Task Init(ProtocolTwitch tc)
    {
        confEntity = tc;
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
    public override async Task<int> Die()
    {
        client.Disconnect();
        client = null;
        return 200;
    }
    public override async Task<int> UpdateConfiguration(ProtocolConfiguration incomingCfg)
    {
        var newConfEntity = incomingCfg as ProtocolTwitch;
        if (newConfEntity != null)
        {
            Console.WriteLine("Twitch Interface was able to cast incoming configuration to a twitch configuration");
            if(newConfEntity.username != confEntity.username || newConfEntity.oauth != confEntity.oauth)
            {
        client.Disconnect();
        confEntity = newConfEntity;
        client.Initialize(new ConnectionCredentials(confEntity.username, confEntity.oauth, capabilities: new Capabilities()));
            }
            return 200;
        }
        else
        {
            Console.Error.WriteLine("update configuration for twitch interface handling {confEntity.Id} given invalid configuration:");
            Console.Error.WriteLine(JsonConvert.SerializeObject(incomingCfg));
            Die();
            return 422;
        }
    }
    private async void Client_OnWhisperReceivedAsync(object sender, OnWhisperReceivedArgs e)
    {
        //data received
        Console.WriteLine($"whisper#{e.WhisperMessage.Username}[{DateTime.Now}][{e.WhisperMessage.DisplayName} [id={e.WhisperMessage.Username}]][msg id: {e.WhisperMessage.MessageId}] {e.WhisperMessage.Message}");

        //translate to internal, upsert
        var m = UpsertMessage(e.WhisperMessage);
        //can't send whispers without giving up cellphone number.
        m.Channel.ChannelType = vassago.Models.Enumerations.ChannelType.DM;
        base.basedot_MessageReceived(m);
    }

    private async void Client_OnMessageReceivedAsync(object sender, OnMessageReceivedArgs e)
    {
        //data eived
        Console.WriteLine($"#{e.ChatMessage.Channel}[{DateTime.Now}][{e.ChatMessage.DisplayName} [id={e.ChatMessage.Username}]][msg id: {e.ChatMessage.Id}] {e.ChatMessage.Message}");

        //translate to internal, upsert
        var m = UpsertMessage(e.ChatMessage);
        m.Channel.ChannelType = vassago.Models.Enumerations.ChannelType.Normal;
        //act on
        base.basedot_MessageReceived(m);
    }

    private void Client_OnConnected(object sender, OnConnectedArgs e)
    {
        Console.WriteLine($"twitch marking selfaccount as seeninchannel {protocolAsChannel.Id}");
        selfAccountInProtocol = UpsertAccount(e.BotUsername, protocolAsChannel);
        selfAccountInProtocol.DisplayName = e.BotUsername;
        Behaver.Instance.MarkSelf(selfAccountInProtocol);
        Console.WriteLine($"Connected to {e.AutoJoinChannel}");

        AttemptJoin(e.BotUsername);
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
        var acc = r.SearchAccount(ui => ui.ExternalId == username && ui.SeenInChannel.ExternalId == inChannel.ExternalId);
        // Console.WriteLine($"upserting twitch account, retrieved {acc?.Id}.");
        if (acc != null)
        {
            Console.WriteLine($"acc's usser: {acc.IsUser?.Id}");
        }
        acc ??= new Account()
        {
            IsUser = r.SearchUser(
                u => u.Accounts.Any(a => a.ExternalId == username && a.Protocol == Protocol))
                ?? new vassago.Models.User()
        };

        acc.Username = username;
        acc.ExternalId = username;
        //acc.IsBot = false? there is a way to tell, but you have to go back through the API
        acc.Protocol = Protocol;
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
        r.RememberAccount(acc);
        inChannel.Users ??= [];
        if (!inChannel.Users.Contains(acc))
        {
            inChannel.Users.Add(acc);
            r.RememberChannel(inChannel);
        }
        return acc;
    }

    private Channel UpsertChannel(string channelName)
    {
        Channel c = r.SearchChannel(ci => ci.ExternalId == channelName
                                             && ci.Protocol == Protocol);
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
        c.Protocol = Protocol;
        c.ParentChannel = protocolAsChannel;
        c.SubChannels = c.SubChannels ?? new List<Channel>();
        c = r.RememberChannel(c);

        var selfAccountInChannel = c.Users?.FirstOrDefault(a => a.ExternalId == selfAccountInProtocol.ExternalId);
        if (selfAccountInChannel == null)
        {
            selfAccountInChannel = UpsertAccount(selfAccountInProtocol.Username, c);
        }

        return c;
    }
    private Channel UpsertDMChannel(string whisperWith)
    {
        Channel c = r.SearchChannel(ci => ci.ExternalId == $"w_{whisperWith}"
                                                     && ci.Protocol == Protocol);
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
        c.Protocol = Protocol;
        c.ParentChannel = protocolAsChannel;
        c.SubChannels = c.SubChannels ?? new List<Channel>();
        c = r.RememberChannel(c);

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
        var m = r.SearchMessage(mi => mi.ExternalId == chatMessage.Id && mi.Protocol == Protocol)
            ?? new()
            {
                Protocol = Protocol,
                Timestamp = (DateTimeOffset)DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };
        m.Content = chatMessage.Message;
        m.ExternalId = chatMessage.Id;
        m.Channel = UpsertChannel(chatMessage.Channel);
        m.Author = UpsertAccount(chatMessage.Username, m.Channel);
        m.MentionsMe = Regex.IsMatch(m.Content?.ToLower(), $"@\\b{selfAccountInProtocol.Username.ToLower()}\\b");
        r.RememberMessage(m);
        return m;
    }
    //n.b., I see you future adam. "we should unify these, they're redundant".
    //ah, but that's the trick, they aren't! twitchlib has a common base class, but
    //none of the features we care about are on it!
    private Message UpsertMessage(WhisperMessage whisperMessage)
    {
        //WhisperMessage.Id corresponds to chatMessage.Id. \*eye twitch*
        var m = r.SearchMessage(mi => mi.ExternalId == whisperMessage.MessageId && mi.Protocol == Protocol)
            ?? new()
            {
                Id = Guid.NewGuid(),
                Protocol = Protocol,
                Timestamp = (DateTimeOffset)DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };
        m.Content = whisperMessage.Message;
        m.ExternalId = whisperMessage.MessageId;
        m.Channel = UpsertDMChannel(whisperMessage.Username);
        m.Author = UpsertAccount(whisperMessage.Username, m.Channel);
        m.MentionsMe = Regex.IsMatch(m.Content?.ToLower(), $"@\\b{selfAccountInProtocol.Username.ToLower()}\\b");
        r.RememberMessage(m);
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
    public override async Task<int> SendMessage(Channel channel, string text)
    {
        client.SendMessage(channel.ExternalId, text);
        return 200;
    }
    public override async Task<int> SendFile(Channel channel, string base64dData, string filename, string accompanyingText)
    {
        return 405;
    }
    public override async Task<int> React(Message message, string reaction)
    {
        return 405;
    }
    public override async Task<int> Reply(Message message, string text)
    {
        client.SendReply(message.Channel.ExternalId, message.ExternalId, text);
        return 200;
    }
}
