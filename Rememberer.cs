namespace vassago;

using System.Linq.Expressions;
using vassago.Models;
using Microsoft.EntityFrameworkCore;

public static class Rememberer
{
    private static readonly SemaphoreSlim dbAccessSemaphore = new(1, 1);
    private static readonly ChattingContext db = new();
    private static List<Channel> channels;
    private static bool channelCacheDirty = true;

    private static void cacheChannels()
    {
        dbAccessSemaphore.Wait();
        channels = db.Channels.ToList();
        foreach (Channel ch in channels)
        {
            if (ch.ParentChannelId != null)
            {
                ch.ParentChannel = channels.FirstOrDefault(c => c.Id == ch.ParentChannelId);
                ch.ParentChannel.SubChannels ??= [];
                ch.ParentChannel.SubChannels.Add(ch);
            }
        }
        channelCacheDirty = false;
        dbAccessSemaphore.Release();
    }

    public static Account SearchAccount(Expression<Func<Account, bool>> predicate)
    {
        Account toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Accounts?.Include(a => a.IsUser)?.FirstOrDefault(predicate);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static List<Account> SearchAccounts(Expression<Func<Account, bool>> predicate)
    {
        List<Account> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Accounts.Where(predicate).ToList();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static Attachment SearchAttachment(Expression<Func<Attachment, bool>> predicate)
    {
        Attachment toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Attachments.FirstOrDefault(predicate);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static Channel SearchChannel(Func<Channel, bool> predicate)
    {
        if(channelCacheDirty)
            Task.Run(() => cacheChannels()).Wait();
        return channels.FirstOrDefault(predicate);
    }
    public static Message SearchMessage(Expression<Func<Message, bool>> predicate)
    {
        Message toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Messages.FirstOrDefault(predicate);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static User SearchUser(Expression<Func<User, bool>> predicate)
    {
        User toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Users.Where(predicate).Include(u => u.Accounts).FirstOrDefault(predicate);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static void RememberAccount(Account toRemember)
    {
        dbAccessSemaphore.Wait();
        toRemember.IsUser ??= new User { Accounts = [toRemember] };
        db.Update(toRemember.IsUser);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public static void RememberAttachment(Attachment toRemember)
    {
        dbAccessSemaphore.Wait();
        toRemember.Message ??= new Message() { Attachments = [toRemember] };
        db.Update(toRemember.Message);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public static Channel RememberChannel(Channel toRemember)
    {
        if(channelCacheDirty)
            Task.Run(() => cacheChannels()).Wait(); //so we always do 2 db trips?
        dbAccessSemaphore.Wait();
        db.Update(toRemember);
        db.SaveChanges();
        dbAccessSemaphore.Release();
        channelCacheDirty = true;
        cacheChannels();
        return toRemember;
    }
    public static void RememberMessage(Message toRemember)
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
    public static void RememberUser(User toRemember)
    {
        dbAccessSemaphore.Wait();
        db.Users.Update(toRemember);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public static void ForgetAccount(Account toForget)
    {
        var user = toForget.IsUser;
        var usersOnlyAccount = user.Accounts?.Count == 1;

        if (usersOnlyAccount)
        {
            Rememberer.ForgetUser(user);
        }
        else
        {
            dbAccessSemaphore.Wait();
            db.Accounts.Remove(toForget);
            db.SaveChanges();
            dbAccessSemaphore.Release();
        }
    }
    public static void ForgetAttachment(Attachment toForget)
    {
        dbAccessSemaphore.Wait();
        db.Attachments.Remove(toForget);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public static void ForgetChannel(Channel toForget)
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
    }
    public static void ForgetMessage(Message toForget)
    {
        dbAccessSemaphore.Wait();
        db.Messages.Remove(toForget);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public static void ForgetUAC(UAC toForget)
    {
        dbAccessSemaphore.Wait();
        db.UACs.Remove(toForget);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public static void ForgetUser(User toForget)
    {
        dbAccessSemaphore.Wait();
        db.Users.Remove(toForget);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
    public static List<Account> AccountsOverview()
    {
        List<Account> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = [.. db.Accounts];
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static List<Channel> ChannelsOverview()
    {
        List<Channel> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = [.. db.Channels.Include(u => u.SubChannels).Include(c => c.ParentChannel)];
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static Account AccountDetail(Guid Id)
    {
        Account toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Accounts.Find(Id);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static Attachment AttachmentDetail(Guid Id)
    {
        Attachment toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Attachments.Find(Id);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static Channel ChannelDetail(Guid Id)
    {
        if(channelCacheDirty)
            Task.Run(() => cacheChannels()).Wait();
        return channels.Find(c => c.Id == Id);
    }
    public static Message MessageDetail(Guid Id)
    {
        Message toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Messages.Find(Id);
        db.Entry(toReturn).Reference(m => m.Channel).Load();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static UAC UACDetail(Guid Id)
    {
        UAC toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.UACs.Find(Id);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static User UserDetail(Guid Id)
    {
        User toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Users.Find(Id);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static List<User> UsersOverview()
    {
        List<User> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Users.ToList();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static List<UAC> UACsOverview()
    {
        List<UAC> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.UACs.Include(uac => uac.Users).Include(uac => uac.Channels).Include(uac => uac.AccountInChannels).ToList();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static UAC SearchUAC(Expression<Func<UAC, bool>> predicate)
    {
        UAC toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.UACs.Include(uac => uac.Users).Include(uac => uac.Channels).Include(uac => uac.AccountInChannels)
            .FirstOrDefault(predicate);
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static List<UAC> MatchUACs(Message message)
    {
        var msgId = message.Id;
        var accId = message.Author.Id;
        var usrId = message.Author.IsUser.Id;
        var chId = message.Channel.Id;

        return SearchUACs(uac => uac.AccountInChannels.FirstOrDefault(aic => aic.Id == accId) != null
                          || uac.Users.FirstOrDefault(usr => usr.Id == usrId) != null
                          || uac.Channels.FirstOrDefault(ch => ch.Id == chId) != null);
    }
    public static List<UAC> SearchUACs(Expression<Func<UAC, bool>> predicate)
    {
        List<UAC> toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.UACs.Include(uac => uac.Users).Include(uac => uac.Channels).Include(uac => uac.AccountInChannels)
            .Where(predicate).ToList();
        dbAccessSemaphore.Release();
        return toReturn;
    }
    public static void RememberUAC(UAC toRemember)
    {
        dbAccessSemaphore.Wait();
        db.Update(toRemember);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
}
