using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using RestSharp;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using vassago.Behavior;
using vassago.Models;

namespace vassago.TwitchInterface;

public class TwitchInterface
{
    internal const string PROTOCOL = "Twitch";
    private bool eventsSignedUp = false;
    private ChattingContext _db;
    private static SemaphoreSlim twitchChannelSetup = new SemaphoreSlim(1, 1);
    private Channel protocolAsChannel;
    TwitchClient client;

    public TwitchInterface()
    {
        _db = new ChattingContext();
    }
    private async Task SetupTwitchChannel()
    {
        await twitchChannelSetup.WaitAsync();

        try
        {
            protocolAsChannel = _db.Channels.FirstOrDefault(c => c.ParentChannel == null && c.Protocol == PROTOCOL);
            if (protocolAsChannel == null)
            {
                protocolAsChannel = new Channel()
                {
                    DisplayName = "twitch (itself)",
                    Permissions = new PermissionSettings()
                    {
                        MeannessFilterLevel = Enumerations.MeannessFilterLevel.Medium,
                        LewdnessFilterLevel = Enumerations.LewdnessFilterLevel.G,
                        MaxTextChars = 500,
                        MaxAttachmentBytes = 0,
                        LinksAllowed = false,
                        ReactionsPossible = false
                    },
                    ExternalId = null,
                    Protocol = PROTOCOL,
                    SubChannels = new List<Channel>()
                };
                protocolAsChannel.SendMessage = (t) => { throw new InvalidOperationException($"twitch itself cannot accept text"); };
                protocolAsChannel.SendFile = (f, t) => { throw new InvalidOperationException($"twitch itself cannot send file"); };
                _db.Channels.Add(protocolAsChannel);
                _db.SaveChanges();
            }
        }
        finally
        {
            twitchChannelSetup.Release();
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

        Console.WriteLine("twitch client connecting...");
        client.Connect();
        Console.WriteLine("twitch client connected");

    }

    private async void Client_OnWhisperReceivedAsync(object sender, OnWhisperReceivedArgs e)
    {
        Console.WriteLine($"whisper#{e.WhisperMessage.Username}[{DateTime.Now}][{e.WhisperMessage.DisplayName} [id={e.WhisperMessage.Username}]][msg id: {e.WhisperMessage.MessageId}] {e.WhisperMessage.Message}");
        var m = UpsertMessage(e.WhisperMessage);
        m.Channel.IsDM = true;

        m.MentionsMe = Regex.IsMatch(e.WhisperMessage.Message?.ToLower(), $"\\b@{e.WhisperMessage.BotUsername.ToLower()}\\b");

        if (await Behaver.Instance.ActOn(m))
        {
            m.ActedOn = true;
        }
        _db.SaveChanges();
    }

    private async void Client_OnMessageReceivedAsync(object sender, OnMessageReceivedArgs e)
    {
        Console.WriteLine($"#{e.ChatMessage.Channel}[{DateTime.Now}][{e.ChatMessage.DisplayName} [id={e.ChatMessage.Username}]][msg id: {e.ChatMessage.Id}] {e.ChatMessage.Message}");
        var m = UpsertMessage(e.ChatMessage);

        m.MentionsMe = Regex.IsMatch(e.ChatMessage.Message?.ToLower(), $"@{e.ChatMessage.BotUsername.ToLower()}\\b") ||
            e.ChatMessage.ChatReply?.ParentUserLogin == e.ChatMessage.BotUsername;

        if (await Behaver.Instance.ActOn(m))
        {
            m.ActedOn = true;
        }
        _db.SaveChanges();
    }

    private async void Client_OnConnected(object sender, OnConnectedArgs e)
    {
        var selfUser = UpsertAccount(e.BotUsername, protocolAsChannel.Id);

        await _db.SaveChangesAsync();
        Behaver.Instance.Selves.Add(selfUser);

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

    private Account UpsertAccount(string username, Guid inChannel)
    {
        var hadToAdd = false;
        var acc = _db.Accounts.FirstOrDefault(ui => ui.ExternalId == username && ui.SeenInChannel.Id == inChannel);
        if (acc == null)
        {
            acc = new Account();
            _db.Accounts.Add(acc);
            hadToAdd = true;
        }
        acc.Username = username;
        acc.ExternalId = username;
        //acc.IsBot =
        acc.Protocol = PROTOCOL;

        if(hadToAdd)
        {
            acc.IsUser = _db.Users.FirstOrDefault(u => u.Accounts.Any(a => a.ExternalId == acc.ExternalId && a.Protocol == acc.Protocol));
            if(acc.IsUser == null)
            {
                acc.IsUser = new vassago.Models.User() { Accounts = new List<Account>() { acc } };
                _db.Users.Add(acc.IsUser);
            }
        }
        return acc;
    }

    private Channel UpsertChannel(string channelName)
    {
        Channel c = _db.Channels.FirstOrDefault(ci => ci.ExternalId == channelName && ci.Protocol == PROTOCOL);
        if (c == null)
        {
            c = new Channel();
            _db.Channels.Add(c);
        }
        c.DisplayName = channelName;
        c.ExternalId = channelName;
        c.IsDM = false;
        c.Messages = c.Messages ?? new List<Message>();
        c.Protocol = PROTOCOL;
        c.ParentChannel = protocolAsChannel;
        c.SubChannels = c.SubChannels ?? new List<Channel>();
        c.SendMessage = (t) => { return Task.Run(() => {client.SendMessage(channelName, t); }); };
        c.SendFile = (f, t) => { throw new InvalidOperationException($"twitch cannot send files");  };
        return c;
        throw new NotImplementedException();
    }
    private Channel UpsertDMChannel(string whisperWith)
    {
        Channel c = _db.Channels.FirstOrDefault(ci => ci.ExternalId == $"w_{whisperWith}" && ci.Protocol == PROTOCOL);
        if (c == null)
        {
            c = new Channel();
            _db.Channels.Add(c);
        }
        c.DisplayName = $"Whisper: {whisperWith}";
        c.ExternalId = $"w_{whisperWith}";
        c.IsDM = true;
        c.Messages = c.Messages ?? new List<Message>();
        c.Protocol = PROTOCOL;
        c.ParentChannel = protocolAsChannel;
        c.SubChannels = c.SubChannels ?? new List<Channel>();
        c.SendMessage = (t) => { return Task.Run(() => {client.SendWhisper(whisperWith, t); }); };
        c.SendFile = (f, t) => { throw new InvalidOperationException($"twitch cannot send files");  };
        return c;
        throw new NotImplementedException();
    }

    private Message UpsertMessage(ChatMessage chatMessage)
    {
        var m = _db.Messages.FirstOrDefault(mi => mi.ExternalId == chatMessage.Id);
        if (m == null)
        {
            m = new Message();
            _db.Messages.Add(m);
            m.Timestamp =(DateTimeOffset)DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        m.Content = chatMessage.Message;
        m.ExternalId = chatMessage.Id;
        m.Channel = UpsertChannel(chatMessage.Channel);
        m.Author = UpsertAccount(chatMessage.Username, m.Channel.Id);
        m.Author.SeenInChannel = m.Channel;

        m.Reply = (t) => { return Task.Run(() => { client.SendReply(chatMessage.Channel, chatMessage.Id, t); }); };
        m.React = (e) => { throw new InvalidOperationException($"twitch cannot react"); };
        return m;
    }
    private Message UpsertMessage(WhisperMessage whisperMessage)
    {
        var m = _db.Messages.FirstOrDefault(mi => mi.ExternalId == whisperMessage.MessageId);
        if (m == null)
        {
            m = new Message();
            _db.Messages.Add(m);
            m.Timestamp =(DateTimeOffset)DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        m.Content = whisperMessage.Message;
        m.ExternalId = whisperMessage.MessageId;
        m.Channel = UpsertDMChannel(whisperMessage.Username);
        m.Channel.IsDM = true;
        m.Author = UpsertAccount(whisperMessage.Username, m.Channel.Id);
        m.Author.SeenInChannel = m.Channel;

        m.Reply = (t) => { return Task.Run(() => {client.SendWhisper(whisperMessage.Username, t); });};
        m.React = (e) => { throw new InvalidOperationException($"twitch cannot react"); };
        return m;
    }

    public string AttemptJoin(string channelTarget)
    {
        client.JoinChannel(channelTarget);
        return "o7";
    }

    internal void AttemptLeave(string channelTarget)
    {
        client.SendMessage(channelTarget, "o7");
        client.LeaveChannel(channelTarget);
    }
}