namespace vassago;

using System.Linq.Expressions;
using vassago.Models;
using Microsoft.EntityFrameworkCore;

public static class Rememberer
{
    private static readonly SemaphoreSlim dbAccessSemaphore = new(1, 1);
    private static readonly ChattingContext db = new();

    public static Account SearchAccount(Expression<Func<Account, bool>> predicate)
    {
        Account toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Accounts.Include(a => a.IsUser).FirstOrDefault(predicate);
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
    public static Channel SearchChannel(Expression<Func<Channel, bool>> predicate)
    {
        Channel toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Channels.FirstOrDefault(predicate);
        dbAccessSemaphore.Release();
        return toReturn;
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
        dbAccessSemaphore.Wait();
        db.Update(toRemember);
        db.SaveChanges();
        dbAccessSemaphore.Release();
        return toRemember;
    }
    public static void RememberMessage(Message toRemember)
    {
        dbAccessSemaphore.Wait();
        toRemember.Channel ??= new() { Messages = [toRemember] };
        db.Update(toRemember.Channel);
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
    public static void ForgetChannel(Channel toForget)
    {
        dbAccessSemaphore.Wait();
        db.Channels.Remove(toForget);
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
    public static Channel ChannelDetail(Guid Id)
    {
        Channel toReturn;
        dbAccessSemaphore.Wait();
        toReturn = db.Channels.Find(Id);
        dbAccessSemaphore.Release();
        return toReturn;
        // .Include(u => u.SubChannels)
        // .Include(u => u.Users)
        // .Include(u => u.ParentChannel);
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
     public static void RememberUAC(UAC toRemember)
    {
        dbAccessSemaphore.Wait();
        db.Update(toRemember);
        db.SaveChanges();
        dbAccessSemaphore.Release();
    }
}
