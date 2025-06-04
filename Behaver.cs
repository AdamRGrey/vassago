namespace vassago;
using gray_messages.chat;
using franz;
using vassago.Behavior;
using vassago.Models;
using vassago.ProtocolInterfaces;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using vassago.ProtocolInterfaces.DiscordInterface;

public class Behaver
{
    private List<Account> SelfAccounts { get; set; } = new List<Account>();
    private User SelfUser { get; set; }
    public static List<vassago.Behavior.Behavior> Behaviors { get; private set; } = new List<vassago.Behavior.Behavior>();
    internal Behaver()
    {
        var subtypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(domainAssembly => domainAssembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(vassago.Behavior.Behavior)) && !type.IsAbstract &&
                type.GetCustomAttributes(typeof(StaticPlzAttribute), false)?.Any() == true)
            .ToList();

        foreach (var subtype in subtypes)
        {
            Behaviors.Add((vassago.Behavior.Behavior)Activator.CreateInstance(subtype));
        }
    }
    static Behaver() { }

    private static readonly Behaver _instance = new Behaver();

    //TODO: you know why I didn't make this a static class? lifecycle issues with the dbcontext. but now that we don't have a stored instance, 
    //no need to have a... *checks over shoulder*... *whispers*: singleton
    public static Behaver Instance
    {
        get { return _instance; }
    }

    public async Task<bool> ActOn(Message message)
    {
        //TODO: this is yet another hit to the database, and a big one. cache them in memory! there needs to be a feasibly-viewable amount, anyway.
        var matchingUACs = Rememberer.MatchUACs(message);
        var behaviorsActedOn = new List<string>();
        foreach (var behavior in Behaviors.ToList())
        {
            //if (!behavior.ShouldAct(message, matchingUACs)) //TODO: this way
            if (!behavior.ShouldAct(message))
            {
                continue;
            }
            behavior.ActOn(message);
            message.ActedOn = true;
            behaviorsActedOn.Add(behavior.ToString());
            Console.WriteLine("acted on, moving forward");
        }
        if (message.ActedOn == false && message.MentionsMe && message.Content.Contains('?') && !Behaver.Instance.SelfAccounts.Any(acc => acc.Id == message.Author.Id))
        {
            Console.WriteLine("providing bullshit nonanswer / admitting uselessness");
            var responses = new List<string>(){
                                @"Well, that's a great question, and there are certainly many different possible answers. Ultimately, the decision will depend on a variety of factors, including your personal interests and goals, as well as any practical considerations (like the economy). I encourage you to do your research, speak with experts and educators, and explore your options before making a decision that's right for you.",
                                @"┐(ﾟ ～ﾟ )┌", @"¯\_(ツ)_/¯", @"╮ (. ❛ ᴗ ❛.) ╭", @"╮(╯ _╰ )╭"
                            };
            Behaver.Instance.SendMessage(message.Channel.Id, responses[Shared.r.Next(responses.Count)]);
            message.ActedOn = true;
            behaviorsActedOn.Add("generic question fallback");
        }
        Rememberer.RememberMessage(message);
        ForwardToKafka(message, behaviorsActedOn, matchingUACs);
        return message.ActedOn;
    }

    internal void ForwardToKafka(Message message, List<string> actedOnBy, List<UAC> matchingUACs)
    {
        var kafkaesque = new chat_message()
        {
            Api_Uri = Shared.API_URL,
            MessageId = message.Id,

            MessageContent = message.Content,
            MentionsMe = message.MentionsMe,
            Timestamp = message.Timestamp,
            AttachmentCount = (uint)(message.Attachments?.Count() ?? 0),

            AccountId = message.Author.Id,
            AccountName = message.Author.DisplayName,

            UserId = message.Author.IsUser.Id,
            UserName = message.Author.IsUser.DisplayName,

            ChannelId = message.Channel.Id,
            ChannelName = message.Channel.DisplayName,
            ChannelProtoocl = message.Channel.Protocol,

            UAC_Matches = matchingUACs.Select(uac => uac.Id).ToList(),
            BehavedOnBy = actedOnBy
        };
        Telefranz.Instance.ProduceMessage(kafkaesque);
    }

    internal bool IsSelf(Guid AccountId)
    {
        var acc = Rememberer.SearchAccount(a => a.Id == AccountId);

        return SelfAccounts.Any(acc => acc.Id == AccountId);
    }

    public void MarkSelf(Account selfAccount)
    {
        if (SelfUser == null)
        {
            SelfUser = selfAccount.IsUser;
        }
        else if (SelfUser != selfAccount.IsUser)
        {
            CollapseUsers(SelfUser, selfAccount.IsUser);
        }
        SelfAccounts = Rememberer.SearchAccounts(a => a.IsUser == SelfUser);
        Rememberer.RememberAccount(selfAccount);
    }

    public bool CollapseUsers(User primary, User secondary)
    {
        if (primary.Accounts == null)
            primary.Accounts = new List<Account>();
        if (secondary.Accounts != null)
            primary.Accounts.AddRange(secondary.Accounts);
        foreach (var a in secondary.Accounts)
        {
            a.IsUser = primary;
        }
        secondary.Accounts.Clear();
        var uacs = Rememberer.SearchUACs(u => u.Users.FirstOrDefault(u => u.Id == secondary.Id) != null);
        if (uacs.Count() > 0)
        {
            foreach (var uac in uacs)
            {
                uac.Users.RemoveAll(u => u.Id == secondary.Id);
                uac.Users.Add(primary);
                Rememberer.RememberUAC(uac);
            }
        }
        Rememberer.ForgetUser(secondary);
        Rememberer.RememberUser(primary);
        return true;
    }
    private ProtocolInterface fetchInterface(Channel ch)
    {
        var walkUp = ch;
        while (walkUp.ParentChannel != null)
        {
            walkUp = walkUp.ParentChannel;
        }
        foreach (var iproto in Shared.ProtocolList)
        {
            if (iproto.SelfChannel.Id == walkUp.Id)
                return iproto;
        }
        return null;
    }
    public async Task<int> SendMessage(Guid channelId, string text)
    {
        var channel = Rememberer.ChannelDetail(channelId);
        if (channel == null)
            return 404;
        var iprotocol = fetchInterface(channel);
        if (iprotocol == null)
            return 404;

        return await iprotocol.SendMessage(channel, text);
    }
    public async Task<int> React(Guid messageId, string reaction)
    {
        Console.WriteLine($"sanity check: behaver is reacting, {messageId}, {reaction}");
        var message = Rememberer.MessageDetail(messageId);
        if (message == null)
        {
            Console.Error.WriteLine($"message {messageId} not found");
            return 404;
        }
        Console.WriteLine($"sanity check: message found.");
        if (message.Channel == null)
        {
            Console.Error.WriteLine($"react is going to fail because message {messageId} has no Channel");
        }
        Console.WriteLine($"sanity check: message has a channel.");
        var iprotocol = fetchInterface(message.Channel);
        if (iprotocol == null)
        {
            Console.WriteLine($"couldn't find protocol for {message.Channel?.Id}");
            return 404;
        }
        Console.WriteLine("I remember this message, i have found a protocol, i am ready to react toit");
        return await iprotocol.React(message, reaction);
    }
    public async Task<int> Reply(Guid messageId, string text)
    {
        var message = Rememberer.MessageDetail(messageId);
        if (message == null)
        {
            Console.WriteLine($"message {messageId} not found");
            return 404;
        }
        var iprotocol = fetchInterface(message.Channel);
        if (iprotocol == null)
        {
            Console.WriteLine($"couldn't find protocol for {message.Channel.Id}");
            return 404;
        }
        return await iprotocol.Reply(message, text);
    }
    public async Task<int> SendFile(Guid channelId, string path, string accompanyingText)
    {
        var channel = Rememberer.ChannelDetail(channelId);
        if (channel == null)
            return 404;
        var iprotocol = fetchInterface(channel);
        if (iprotocol == null)
            return 404;

        return await iprotocol.SendFile(channel, path, accompanyingText);
    }
}
