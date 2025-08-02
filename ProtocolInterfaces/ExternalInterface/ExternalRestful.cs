namespace vassago.ProtocolInterfaces;
using vassago.Models;
using vassago.ProtocolInterfaces;
using static vassago.Models.Enumerations;

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
            protocolAsChannel = r.RememberChannel(protocolAsChannel);
        }
    }
    public override async Task<int> SendMessage(Channel channel, string text)
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.SendMessage,
            ChannelId = channel.Id,
            Text = text
        });
        return 200;
    }
    public override async Task<int> SendFile(Channel channel, string base64dData, string filename, string accompanyingText)
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.SendFile,
            ChannelId = channel.Id,
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
            MessageId = message.Id,
            Text = reaction
        });
        return 200;
    }
    public override async Task<int> Reply(Message message, string text)
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.Reply,
            MessageId = message.Id,
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
        var containingChannel = getMyChannel(channelExternalId)?.Item1;
        if (containingChannel == null) return 404;
        var authoringAccount = containingChannel.Users?.FirstOrDefault(a => a.ExternalId == authorExternalId);
        if (authoringAccount == null) return 404;
        Console.WriteLine($"received message; author: {message.Author.DisplayName}, {message.Author.Id}. messageid:{message.Id}");
        r.RememberMessage(message);
        base.basedot_MessageReceived(message);
        return 202;
    }
    public async Task<int> ExternalMessageUpdate(Message message, string authorExternalId, string channelExternalId)
    {
        if (String.IsNullOrWhiteSpace(message.ExternalId) || string.IsNullOrWhiteSpace(authorExternalId) || string.IsNullOrWhiteSpace(channelExternalId))
            return 400;
        var containingChannel = getMyChannel(channelExternalId)?.Item1;
        if (containingChannel == null) return 404;
        var authoringAccount = containingChannel.Users?.FirstOrDefault(a => a.ExternalId == authorExternalId);
        if (authoringAccount == null) return 404;

        Task.Run(() =>
        {
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
                msg.TranslatedContent = message.TranslatedContent;
                r.RememberMessage(msg);
                base.basedot_MessageUpdated(msg);
            }
        });
        return 202;
    }
    ///<summary>
    ///you've joined a channel. Tip: if, like discord, you learn about a channel, and consequently walk
    ///up the hierarchy... go to the top and "join" your way down. those external Ids, in order, are your "channel lineage"
    ///if any of them don't exist, error
    ///</summary>
    public async Task<int> ExternalChannelJoin(Channel channel, List<string> channelLineage)
    {
        if (String.IsNullOrWhiteSpace(channel?.ExternalId)) return 400;

        var found = getMyChannel(channel.ExternalId, channelLineage);
        if(found.Item1 != null) return 409;

        var immediateParent = found.Item2.Last();
        immediateParent.SubChannels ??= [];
        immediateParent.SubChannels.Add(channel);
        channel.ParentChannel = immediateParent;
        r.RememberChannel(channel);
        Console.WriteLine($"joined channel {channel.DisplayName}/{channel.ExternalId} - {channel.Id}");

        base.basedot_ChannelJoined(channel);
        return 201;
    }
    public async Task<int> ExternalChannelUpdate(Channel updatedChannel, List<string> channelLineage)
    {
        var otherTasks = new List<Task>();
        if ((protocolAsChannel.SubChannels?.Count ?? 0) == 0) return 404;
        var found = getMyChannel(updatedChannel.ExternalId, channelLineage);
        var dbChannel = found.Item1;
        if (dbChannel == null) return 404;

        if (String.Join('/', found.Item2) != String.Join('/', channelLineage))
        {
            var immediateParent = found.Item2.Last();
            var formerParent = dbChannel.ParentChannel;
            formerParent.SubChannels.Remove(dbChannel);
            dbChannel.ParentChannel = immediateParent;
            immediateParent.SubChannels ??= [];
            immediateParent.SubChannels.Add(dbChannel);
            otherTasks.Add(Task.Run(() => r.RememberChannel(formerParent)));
            otherTasks.Add(Task.Run(() => r.RememberChannel(immediateParent)));
        }

        dbChannel.ChannelType = updatedChannel.ChannelType;
        dbChannel.DisplayName = updatedChannel.DisplayName;
        dbChannel.LinksAllowed = updatedChannel.LinksAllowed;
        dbChannel.MaxAttachmentBytes = updatedChannel.MaxAttachmentBytes;
        dbChannel.MaxTextChars = updatedChannel.MaxTextChars;
        dbChannel.ReactionsPossible = updatedChannel.ReactionsPossible;
        otherTasks.Add(Task.Run(() => r.RememberChannel(dbChannel)));

        foreach (var t in otherTasks)
        {
            await t;
        }
        base.basedot_ChannelUpdated(dbChannel);
        return 200;
    }
    public async Task<int> ExternalAccountCreate(Account account, string channelExternalId)
    {
        var found = getMyChannel(channelExternalId);
        var dbChannel = found.Item1;
        if (dbChannel == null) return 404;

        var already = dbChannel.Users?.FirstOrDefault(a => a.ExternalId == account.ExternalId);
        if(already != null)
            return 409;

        dbChannel.Users ??=[];
        dbChannel.Users.Add(account);
        var channelTask = Task.Run(() => r.RememberChannel(dbChannel));

        if(account.IsUser == null)
        {
            account.IsUser = protocolAsChannel.Users?.FirstOrDefault(acc => acc.ExternalId == account.ExternalId)?.IsUser;
            if(account.IsUser == null)
            {
                account.IsUser = new (){
                    Accounts = [account]
                };
            }
        }
        else{
            account.IsUser.Accounts ??= [];
            if(!account.IsUser.Accounts.Contains(account))
                account.IsUser.Accounts.Add(account);
        }
        await channelTask;
        r.RememberAccount(account);
        base.basedot_AccountMet(account);
        return 200;
    }
    public async Task<int> ExternalAccountUpdate(Account account, string channelExternalId)
    {
        var found = getMyChannel(channelExternalId);
        var dbChannel = found.Item1;
        if (dbChannel == null) return 404;

        var already = dbChannel.Users?.FirstOrDefault(a => a.ExternalId == account.ExternalId);
        if(already == null)
            return 404; //if you need to add an account to a channel... *create* it in that channel.

        already.DisplayName = account.DisplayName;
        already.IsBot = account.IsBot;
        already.Username = account.Username;

        r.RememberAccount(account);
        base.basedot_AccountUpdated(already);
        return 200;
    }
    private Tuple<Channel, List<Channel>> getMyChannel(string channelExternalId, List<string> lineageHint)
    {
        var pathDown = new List<Channel>() { protocolAsChannel };
        if (lineageHint?.Count > 0) foreach (var externalId in lineageHint)
        {
            var child = pathDown.Last().SubChannels?.FirstOrDefault(c => c.ExternalId == externalId);
            if (child == null)
            {
                return new Tuple<Channel, List<Channel>>(null, pathDown);
            }
            else
            {
                pathDown.Add(child);
            }
        }
        var immediateParent = pathDown.Last();

        var dbChannel = immediateParent.SubChannels.FirstOrDefault(c => c.ExternalId == channelExternalId);
        if (dbChannel == null)
        {
            return getMyChannel(channelExternalId);
        }
        else
        {
            //this is the happy path  - found channel, exactly provided path
            return new Tuple<Channel, List<Channel>>(dbChannel, pathDown);
        }
    }
    private Tuple<Channel, List<Channel>> getMyChannel(string channelExternalId)
    {
        //we've determined that the channel lineage is not what we expect. is this even a channel we know about?
        var dbChannel = r.SearchChannel(c => c.ExternalId == channelExternalId);
        //nope. /shrug
        if (dbChannel == null) return new Tuple<Channel, List<Channel>>(null, null);

        //ok it is *a* channel... but is it *our* channel?
        var walkUp = dbChannel.ParentChannel;
        var loopCheckBreadcrumbs = new List<Channel>();
        while (walkUp != protocolAsChannel && walkUp != null && !loopCheckBreadcrumbs.Contains(walkUp))
        {
            loopCheckBreadcrumbs.Add(walkUp);
            walkUp = walkUp.ParentChannel;
        }
        if (walkUp == protocolAsChannel)
        {
            loopCheckBreadcrumbs.Reverse();
            //we've determined this is a channel. it's our channel. probably reorganization / channel lineage change.
            return new Tuple<Channel, List<Channel>>(null, loopCheckBreadcrumbs);
        }
        else
        {
            return new Tuple<Channel, List<Channel>>(null, null);
        }
    }
    // public User UserFor(Account account)
    // {
    //     //i should probably do this in all of these because i'll need it all the time and it's a compl- wait. it's a 1-liner.
    //     return protocolAsChannel.Users?.FirstOrDefault(acc => acc.ExternalId == account.ExternalId)?.IsUser;
    // }
}
public class ExternalCommand
{
    public ExternalCommandType Type { get; set; }
    public Guid ChannelId { get; set; }
    public Guid MessageId { get; set; }
    ///<summary>
    ///or for reactions, reaction. for files, this will be the accompanying text.
    ///</summary>
    public string Text { get; set; }
    public string FileName { get; set; }
    public string FileData { get; set; }
}
