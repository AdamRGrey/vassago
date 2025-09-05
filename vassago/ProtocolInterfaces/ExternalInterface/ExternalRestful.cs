namespace vassago.ProtocolInterfaces;
using vassago.Models;
using vassago.ProtocolInterfaces;
using static vassago.Models.Enumerations;
using System.Linq;

/* ok so. internal interfaces are started and stopped.
 * external interfaces *may* not be running. we connect and disconnect. They may also permanently die.
 * what if they perma-die while vassago is down? /shrug oh well.
 *
 * one "external" "protcol" is one host. so if I put the whole damn thing in a greasemoney script, that might be 1 protocol.
 * but if I need to switch to Go for a matrix client (grumble grumble), that would be 1 proto that may have many channels.
 */

public class ExternalRestful : ProtocolInterface
{
    private Channel protocolAsChannel;
    public override Channel SelfChannel { get => protocolAsChannel; }
    private ProtocolExternal confEntity;
    public override ProtocolConfiguration ConfigurationEntity { get => confEntity; }
    public static new string Protocol { get => "external"; }
    public List<ExternalCommand> CommandQueue { get; set; }

    public async Task Init(ProtocolExternal cfg)
    {
        confEntity = cfg;
        protocolAsChannel = r.SearchChannel(c => c.ParentChannel == null && c.Protocol == Protocol && c.ExternalId == cfg.ExternalId);
        if (protocolAsChannel == null)
        {
            protocolAsChannel = new Channel()
            {
                ExternalId = cfg.ExternalId,
                Protocol = ExternalRestful.Protocol
            };
            //Console.WriteLine("[ExternalRestful.Init] creating new channel");
            protocolAsChannel = r.RememberChannel(protocolAsChannel);
        }
        else
        {
            //Console.WriteLine("[ExternalRestful.Init] found old channel");
        }
    }
    public override async Task<int> SendMessage(Channel channel, string text)
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.SendMessage,
            ChannelId = channel.ExternalId,
            Text = text
        });
        return 200;
    }
    public override async Task<int> SendFile(Channel channel, string base64dData, string filename, string accompanyingText)
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.SendFile,
            ChannelId = channel.ExternalId,
            Text = accompanyingText,
            FileData = base64dData,
            FileName = filename
        });
        return 200;
    }
    public override async Task<int> React(Message message, string reaction)
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.React,
            MessageId = message.ExternalId,
            Text = reaction,
            ChannelId = message.Channel.ExternalId
        });
        return 200;
    }
    public override async Task<int> Reply(Message message, string text)
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.Reply,
            MessageId = message.ExternalId,
            Text = text
        });
        return 200;
    }
    public override async Task<int> UpdateConfiguration(ProtocolConfiguration newCfg)
    {
        //we don't actually do anything. (here in restful style. other styles might care.)
        return 405;
    }
    public override async Task<int> Die()
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.Die
        });
        //TODO: also, die. ...wait so, if i get told internally to die..
        //WAIT is die during the lifetime of the program? or is it permanent?
        //it seems like right now, external is expected to play it back???//TODO: sort that out
        return 200;
    }
    ///<summary>
    ///external is telling me that it received a message.
    ///</summary>
    ///<paramref name="message">a theoretical new message in vassago format</paramref>
    ///<paramref name="channel">a channel you've set up earlier</paramref>
    public async Task<int> ExternalMessageReceive(Message message, string authorExternalId, string channelExternalId)
    {
        var containingChannel = getMyChannel(channelExternalId);
        if (containingChannel == null) return 404;
        message.Channel = containingChannel;
        var authoringAccount = containingChannel.Users?.FirstOrDefault(a => a.ExternalId == authorExternalId);
        if (authoringAccount == null) return 404;
        message.Author = authoringAccount;

        //Console.WriteLine($"received message; author: {message.Author.DisplayName}, {message.Author.Id}. messageid:{message.Id}");
        r.RememberMessage(message);
        base.basedot_MessageReceived(message);
        return 202;
    }
    public async Task<int> ExternalMessageUpdate(Message message, string authorExternalId, string channelExternalId)
    {
        if (String.IsNullOrWhiteSpace(message.ExternalId) || string.IsNullOrWhiteSpace(authorExternalId) || string.IsNullOrWhiteSpace(channelExternalId))
            return 400;
        var containingChannel = getMyChannel(channelExternalId);
        if (containingChannel == null) return 404;
        var authoringAccount = containingChannel.Users?.FirstOrDefault(a => a.ExternalId == authorExternalId);
        if (authoringAccount == null) return 404;

        var msg = r.SearchMessage(m => m.ChannelId == containingChannel.Id && m.Author.Id == authoringAccount.Id && m.ExternalId == message.ExternalId);
        if (msg == null)
        {
            Console.Error.WriteLine($"attempt to update {message.ExternalId}, apparently by {authoringAccount.Id} in channel {containingChannel.Id}, but i don't think that message is in that channel? oh well.");
        }
        else
        {
            msg.Attachments = message.Attachments;
            msg.Content = message.Content;
            msg.MentionsMe = message.MentionsMe;
            msg.Timestamp = message.Timestamp;
            //no no, *i* translate it?
            msg.TranslatedContent = message.TranslatedContent;
            r.RememberMessage(msg);
            base.basedot_MessageUpdated(msg);
        }
        return 200;
    }
    ///<summary>
    ///you've joined a channel. Tip: if, like discord, you learn about a channel, and consequently walk
    ///up the hierarchy... go to the top and "join" your way down. those external Ids, in order, are your "channel lineage"
    ///also, if you're "top level" (other than the channel) - the parentChannelId is your overall Id.
    ///</summary>
    public async Task<int> ExternalChannelJoin(Channel channel, string parentChannelId)
    {
        //Console.WriteLine($"[externalrestful.ExternalChannelJoin] for {channel?.ExternalId}");
        if (String.IsNullOrWhiteSpace(channel?.ExternalId))
        {
            //Console.Error.WriteLine($"[externalrestful.ExternalChannelJoin] no external id specified");
            return 400;
        }

        var found = getMyChannel(channel.ExternalId);
        //Console.WriteLine($"[externalrestful.ExternalChannelJoin] my channel found? {found != null}");
        if (found != null)
        {
            //Console.Error.WriteLine($"[externalrestful.ExternalChannelJoin] already in a channel with that ID");
            return 409;
        }

        var immediateParent = getMyChannel(parentChannelId);
        immediateParent.SubChannels ??= [];
        immediateParent.SubChannels.Add(channel);
        channel.ParentChannel = immediateParent;
        r.RememberChannel(channel);
        //Console.WriteLine($"[externalrestful.ExternalChannelJoin] joined channel {channel.DisplayName}/{channel.ExternalId} - {channel.Id}");

        base.basedot_ChannelJoined(channel);
        return 201;
    }
    public async Task<int> ExternalChannelUpdate(Channel updatedChannel, string parentChannelId)
    {
        //Console.WriteLine("[ExternalChannelUpdate]");
        var otherTasks = new List<Task>();
        protocolAsChannel = r.SearchChannel(c => c.Id == protocolAsChannel.Id);

        string.IsNullOrWhiteSpace(parentChannelId);
        parentChannelId = protocolAsChannel.ExternalId;

        var dbChannel = getMyChannel(updatedChannel.ExternalId);
        if (dbChannel == null)
        {
            Console.Error.WriteLine($"[ExternalChannelUpdate] - couldn't find that channel");
            return 404;
        }

        if (dbChannel != protocolAsChannel && dbChannel.ParentChannel?.ExternalId != parentChannelId)
        {
            if (string.IsNullOrWhiteSpace(parentChannelId))
            {
                Console.Error.WriteLine("parent channel id not specified.");
                return 400;
            }

            //Console.WriteLine($"[ExternalChannelUpdate] - reorganizing, {dbChannel.ParentChannel?.ExternalId} --> {parentChannelId}");
            var adoptiveParent = getMyChannel(parentChannelId);
            if (adoptiveParent == null)
            {
                Console.Error.WriteLine($"[ExternalChannelUpdate] - adoptive parent not found!");
                return 400;
            }
            var formerParent = dbChannel.ParentChannel;
            if (formerParent != null)
            {
                formerParent.SubChannels.Remove(dbChannel);
                otherTasks.Add(rememberChannelTask(formerParent));
            }
            dbChannel.ParentChannel = adoptiveParent;
            adoptiveParent.SubChannels ??= [];
            adoptiveParent.SubChannels.Add(dbChannel);
            otherTasks.Add(rememberChannelTask(adoptiveParent));
        }
        else
        {
            //Console.WriteLine($"[ExternalChannelUpdate] - no need to reorganize");
        }

        dbChannel.ChannelType = updatedChannel.ChannelType;
        dbChannel.DisplayName = updatedChannel.DisplayName;
        dbChannel.LinksAllowed = updatedChannel.LinksAllowed;
        dbChannel.MaxAttachmentBytes = updatedChannel.MaxAttachmentBytes;
        dbChannel.MaxTextChars = updatedChannel.MaxTextChars;
        dbChannel.ReactionsPossible = updatedChannel.ReactionsPossible;
        dbChannel.LewdnessFilterLevel = updatedChannel.LewdnessFilterLevel;
        dbChannel.MeannessFilterLevel= updatedChannel.MeannessFilterLevel;
        //Console.WriteLine($"[ExternalChannelUpdate] - stuff updated. remembering.");
        otherTasks.Add(rememberChannelTask(dbChannel));

        foreach (var t in otherTasks)
            await t;
        //Console.WriteLine($"[ExternalChannelUpdate] - tasks ready. firing event.");
        base.basedot_ChannelUpdated(dbChannel);
        return 200;
    }
    private async Task rememberChannelTask(Channel channel)
    {
        r.RememberChannel(channel);
    }
    public async Task<int> ExternalAccountCreate(Account account, string channelExternalId)
    {
        var dbChannel = getMyChannel(channelExternalId);
        if (dbChannel == null) return 404;

        var already = dbChannel.Users?.FirstOrDefault(a => a.ExternalId == account.ExternalId);
        if (already != null)
            return 409;

        dbChannel.Users ??= [];
        dbChannel.Users.Add(account);
        var channelTask = rememberChannelTask(dbChannel);

        if (account.IsUser == null)
        {
            account.IsUser = protocolAsChannel.Users?.FirstOrDefault(acc => acc.ExternalId == account.ExternalId)?.IsUser;
            if (account.IsUser == null)
            {
                account.IsUser = new()
                {
                    Accounts = [account]
                };
            }
        }
        else
        {
            account.IsUser.Accounts ??= [];
            if (!account.IsUser.Accounts.Contains(account))
                account.IsUser.Accounts.Add(account);
        }
        await channelTask;
        r.RememberAccount(account);
        base.basedot_AccountMet(account);
        return 200;
    }
    public async Task<int> ExternalAccountUpdate(Account account, string channelExternalId)
    {
        var dbChannel = getMyChannel(channelExternalId);
        if (dbChannel == null) return 404;

        var already = dbChannel.Users?.FirstOrDefault(a => a.ExternalId == account.ExternalId);
        if (already == null)
            return 404; //if you need to add an account to a channel... *create* it in that channel.

        already.DisplayName = account.DisplayName;
        already.IsBot = account.IsBot;
        already.Username = account.Username;

        r.RememberAccount(account);
        base.basedot_AccountUpdated(already);
        return 200;
    }
    private Channel getMyChannel(string channelExternalId)
    {
        var dbChannel = r.SearchChannel(c => c.ExternalId == channelExternalId);
        //Console.WriteLine($"[getMyChannel]({channelExternalId}) - found channel? {dbChannel != null}");
        if (dbChannel == null) return null;

        //ok it is *a* channel... but is it *our* channel?
        //Console.WriteLine($"[getMyChannel] - the goal is to find {protocolAsChannel.ExternalId} (interally known as {protocolAsChannel.Id}");
        var walkUp = dbChannel;
        var loopCheckBreadcrumbs = new List<Channel>();
        while (!loopCheckBreadcrumbs.Contains(walkUp))
        {
            loopCheckBreadcrumbs.Add(walkUp);
            if (walkUp == null)
            {
                Console.WriteLine($"[getMyChannel] passed the top (couldn't find channel)");
                return null;
            }
            if (walkUp.Id == protocolAsChannel.Id)
            {
                protocolAsChannel = walkUp;
                //Console.WriteLine($"[getMyChannel] - walking up, did we hit the top? {walkUp.Id == protocolAsChannel.Id} did we *pass* the top? {walkUp == null}");
                loopCheckBreadcrumbs.Reverse();
                //Console.WriteLine($"[getMyChannel] found channel along {string.Join('/', from c in loopCheckBreadcrumbs select c.ExternalId)}");
                return dbChannel;
            }

            //Console.WriteLine($"[getMyChannel] - walking up, stepping from {walkUp.ExternalId} (internally known as {walkUp.Id}) to its parent...");
            walkUp = walkUp.ParentChannel;
        }

        //Console.WriteLine($"[getMyChannel] failed to find channel, probably found a loop");
        return null;
    }
}
public class ExternalCommand
{
    public ExternalCommandType Type { get; set; }
    public string ChannelId { get; set; }
    public string MessageId { get; set; }
    ///<summary>
    ///or for reactions, reaction. for files, this will be the accompanying text.
    ///</summary>
    public string Text { get; set; }
    public string FileName { get; set; }
    public string FileData { get; set; }
}
