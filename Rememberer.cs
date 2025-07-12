namespace vassago;

using System.Linq.Expressions;
using vassago.Models;
using Microsoft.EntityFrameworkCore;

public class Rememberer
{
    private readonly SemaphoreSlim dbAccessSemaphore = new(1, 1);
    private readonly ChattingContext db = new();
    private List<Channel> channels;
    private bool channelCacheDirty = true;
    private Rememberer() { }
    private static Rememberer _instance = null;
    public static Rememberer Instance
    {
        get
        {
            if (_instance == null)
            {

                lock (instantiationLock)
                {
                    if (_instance == null)
                    {
                        _instance = new Rememberer();
                    }
                }
            }
            return _instance;
        }
    }
    private static readonly object instantiationLock = new();

    private void cacheChannels()
    {
        dbAccessSemaphore.Wait();
        channels = db.Channels.ToList();
        foreach (Channel ch in channels)
        {
            if (ch.ParentChannelId != null)
            {
                ch.ParentChannel = channels.FirstOrDefault(c => c.Id == ch.ParentChannelId);
                ch.ParentChannel.SubChannels ??= [];
                if (!ch.ParentChannel.SubChannels.Contains(ch))
                {
                    ch.ParentChannel.SubChannels.Add(ch);
                }
            }
            ch.SubChannels ??= [];
        }
        channelCacheDirty = false;
        dbAccessSemaphore.Release();
    }

    public Account SearchAccount(Expression<Func<Account, bool>> predicate)
    {
        Account toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Accounts?.Include(a => a.IsUser)?.FirstOrDefault(predicate);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public List<Account> SearchAccounts(Expression<Func<Account, bool>> predicate)
    {
        List<Account> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Accounts.Where(predicate).ToList();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public Attachment SearchAttachment(Expression<Func<Attachment, bool>> predicate)
    {
        Attachment toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Attachments.FirstOrDefault(predicate);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public Channel SearchChannel(Func<Channel, bool> predicate)
    {
        if (channelCacheDirty)
            Task.Run(() => cacheChannels()).Wait();
        return channels.FirstOrDefault(predicate);
    }
    public Message SearchMessage(Expression<Func<Message, bool>> predicate)
    {
        Message toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Messages.FirstOrDefault(predicate);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public List<Message> SearchMessages(Expression<Func<Message, bool>> predicate)
    {
        List<Message> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Messages.Where(predicate).ToList();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public User SearchUser(Expression<Func<User, bool>> predicate)
    {
        User toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Users.Where(predicate).Include(u => u.Accounts).FirstOrDefault(predicate);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public void RememberAccount(Account toRemember)
    {
        dbAccessSemaphore.Wait();
        toRemember.IsUser ??= new User { Accounts = [toRemember] };
        db.Update(toRemember.IsUser);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public void RememberAttachment(Attachment toRemember)
    {
        dbAccessSemaphore.Wait();
        toRemember.Message ??= new Message() { Attachments = [toRemember] };
        db.Update(toRemember.Message);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public Channel RememberChannel(Channel toRemember)
    {
        if (channelCacheDirty)
            Task.Run(() => cacheChannels()).Wait(); //so we always do 2 db trips?
        dbAccessSemaphore.Wait();
        db.Update(toRemember);
        db.SaveChanges();
        dbAccessSemaphore.Release();
        channelCacheDirty = true;
        cacheChannels();
        return toRemember;
    }
    public void RememberMessage(Message toRemember)
    {
        dbAccessSemaphore.Wait();
        toRemember.Channel ??= new();
        toRemember.Channel.Messages ??= [];
        if (!toRemember.Channel.Messages.Contains(toRemember))
        {
            toRemember.Channel.Messages.Add(toRemember);
            db.Update(toRemember.Channel);
        }
        db.Update(toRemember);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public void RememberUser(User toRemember)
    {
        dbAccessSemaphore.Wait();
        db.Users.Update(toRemember);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public void ForgetAccount(Account toForget)
    {
        var user = toForget.IsUser;
        var usersOnlyAccount = user.Accounts?.Count == 1;

        if (usersOnlyAccount)
        {
            ForgetUser(user);
        }
        else
        {
            dbAccessSemaphore.Wait();
            db.Accounts.Remove(toForget);
            db.SaveChanges();
            dbAccessSemaphore.Release();
        }
    }
    public void ForgetAttachment(Attachment toForget)
    {
        dbAccessSemaphore.Wait();
        db.Attachments.Remove(toForget);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public void ForgetChannel(Channel toForget)
    {
        if (toForget.SubChannels?.Count > 0)
        {
            foreach (var childChannel in toForget.SubChannels.ToList())
            {
                ForgetChannel(childChannel);
            }
        }
        if (toForget.Users?.Count > 0)
        {
            foreach (var account in toForget.Users.ToList())
            {
                ForgetAccount(account);
            }
        }
        dbAccessSemaphore.Wait();
        db.Channels.Remove(toForget);
        db.SaveChanges();
        dbAccessSemaphore.Release();
        channelCacheDirty = true;
        cacheChannels();
    }
    public void ForgetMessage(Message toForget)
    {
        dbAccessSemaphore.Wait();
        db.Messages.Remove(toForget);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public void ForgetUAC(UAC toForget)
    {
        dbAccessSemaphore.Wait();
        db.UACs.Remove(toForget);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public void ForgetUser(User toForget)
    {
        dbAccessSemaphore.Wait();
        db.Users.Remove(toForget);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public List<Account> AccountsOverview()
    {
        List<Account> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = [.. db.Accounts];
        dbAccessSemaphore.Release();
        return toReturn;
    }
    ///<summary>
    ///intentionally does not include Users; to help search for orphaned accounts.
    ///</summary>
    public List<Channel> ChannelsOverview()
    {
        if (channelCacheDirty)
            Task.Run(() => cacheChannels()).Wait();
        return channels.ToList();
    }
    public Account AccountDetail(Guid Id)
    {
        Account toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Accounts.Find(Id);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public Attachment AttachmentDetail(Guid Id)
    {
        Attachment toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Attachments.Find(Id);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public Channel ChannelDetail(Guid Id, bool accounts = true, bool messages = false)
    {
        if (channelCacheDirty)
            Task.Run(() => cacheChannels()).Wait();
        var ch = channels.Find(c => c.Id == Id);
        if (accounts)
            ch.Users = SearchAccounts(a => a.SeenInChannel == ch);
        if (messages)
            ch.Messages = SearchMessages(m => m.ChannelId == ch.Id);
        return ch;
    }
    public Message MessageDetail(Guid Id)
    {
        Message toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Messages.Find(Id);
        db.Entry(toReturn).Reference(m => m.Channel).Load();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public UAC UACDetail(Guid Id)
    {
        UAC toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.UACs.Find(Id);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public User UserDetail(Guid Id)
    {
        User toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Users.Find(Id);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public List<User> UsersOverview()
    {
        List<User> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Users.ToList();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public List<UAC> UACsOverview()
    {
        List<UAC> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.UACs.Include(uac => uac.Users).Include(uac => uac.Channels).Include(uac => uac.AccountInChannels).ToList();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public UAC SearchUAC(Expression<Func<UAC, bool>> predicate)
    {
        UAC toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.UACs.Include(uac => uac.Users).Include(uac => uac.Channels).Include(uac => uac.AccountInChannels)
            .FirstOrDefault(predicate);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public List<UAC> MatchUACs(Message message)
    {
        var msgId = message.Id;
        var accId = message.Author.Id;
        var usrId = message.Author.IsUser.Id;
        var chId = message.Channel.Id;

        return SearchUACs(uac => uac.AccountInChannels.FirstOrDefault(aic => aic.Id == accId) != null
                          || uac.Users.FirstOrDefault(usr => usr.Id == usrId) != null
                          || uac.Channels.FirstOrDefault(ch => ch.Id == chId) != null);
    }
    public List<UAC> SearchUACs(Expression<Func<UAC, bool>> predicate)
    {
        List<UAC> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.UACs.Include(uac => uac.Users).Include(uac => uac.Channels).Include(uac => uac.AccountInChannels)
            .Where(predicate).ToList();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public void RememberUAC(UAC toRemember)
    {
        dbAccessSemaphore.Wait();
        db.Update(toRemember);
        db.SaveChanges();
        dbAccessSemaphore.Release();
        if (toRemember.Channels?.Count() > 0)
            cacheChannels();
    }
    public Configuration Configuration()
    {
        dbAccessSemaphore.Wait();
        var toReturn = db.Configurations.FirstOrDefault();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public void RememberConfiguration(Configuration conf)
    {
        dbAccessSemaphore.Wait();
        db.Update(conf);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public Webhook Webhook(Guid id)
    {
        Webhook toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Webhooks.Include(wh => wh.Uac).FirstOrDefault(wh => wh.Id == id);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public List<Webhook> Webhooks()
    {
        List<Webhook> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Webhooks.Include(wh => wh.Uac).ToList();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public void RememberWebhook(Webhook wh)
    {
        dbAccessSemaphore.Wait();
        db.Update(wh);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public void ForgetWebhook(Guid id)
    {
        dbAccessSemaphore.Wait();
        db.Remove(db.Webhooks.Find(id));
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public void ForgetWebhook(Webhook wh)
    {
        dbAccessSemaphore.Wait();
        db.Remove(wh);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
}
