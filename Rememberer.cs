namespace vassago;

using System.Linq.Expressions;
using vassago.Models;
using Microsoft.EntityFrameworkCore;

public static class Rememberer
{
    private static readonly ChattingContext db = new();
    public static Account SearchAccount(Expression<Func<Account, bool>> predicate)
    {
        return db.Accounts.Include(a => a.IsUser).FirstOrDefault(predicate);
    }
    public static List<Account> SearchAccounts(Expression<Func<Account, bool>> predicate)
    {
        return db.Accounts.Where(predicate).ToList();
    }
    public static Attachment SearchAttachment(Expression<Func<Attachment, bool>> predicate)
    {
        return db.Attachments.FirstOrDefault(predicate);
    }
    public static Channel SearchChannel(Expression<Func<Channel, bool>> predicate)
    {
        return db.Channels.FirstOrDefault(predicate);
    }
    public static Message SearchMessage(Expression<Func<Message, bool>> predicate)
    {
        return db.Messages.FirstOrDefault(predicate);
    }
    public static User SearchUser(Expression<Func<User, bool>> predicate)
    {
        return db.Users.Include(u => u.Accounts).FirstOrDefault(predicate);
    }
    public static void RememberAccount(Account toRemember)
    {
        toRemember.IsUser ??= new User{ Accounts = [toRemember]};
        db.Update(toRemember.IsUser);
        db.SaveChanges();
    }
    public static void RememberAttachment(Attachment toRemember)
    {
        toRemember.Message ??= new Message() { Attachments = [toRemember]};
        db.Update(toRemember.Message);
        db.SaveChanges();
    }
    public static Channel RememberChannel(Channel toRemember)
    {
        db.Update(toRemember);
        db.SaveChanges();
        return toRemember;
    }
    public static void RememberMessage(Message toRemember)
    {
        toRemember.Channel ??= new (){ Messages = [toRemember] };
        db.Update(toRemember.Channel);
        db.SaveChanges();
    }
    public static void RememberUser(User toRemember)
    {
        db.Users.Update(toRemember);

        db.SaveChanges();
    }
    public static void ForgetAccount(Account toForget)
    {
        var user = toForget.IsUser;
        var usersOnlyAccount = user.Accounts?.Count == 1;
                
        if(usersOnlyAccount)
        {
            Rememberer.ForgetUser(user);
        }
        else
        {
            db.Accounts.Remove(toForget);
            db.SaveChanges();
        }
    }
    public static void ForgetChannel(Channel toForget)
    {
        db.Channels.Remove(toForget);

        db.SaveChanges();
    }
    public static void ForgetUser(User toForget)
    {
        db.Users.Remove(toForget);

        db.SaveChanges();
    }
    public static List<Account> AccountsOverview()
    {
        return [..db.Accounts];
    }
    public static List<Channel> ChannelsOverview()
    {
        return [..db.Channels.Include(u => u.SubChannels).Include(c => c.ParentChannel)];
    }
    public static Channel ChannelDetail(Guid Id)
    {
        return db.Channels.Find(Id);
            // .Include(u => u.SubChannels)
            // .Include(u => u.Users)
            // .Include(u => u.ParentChannel);
    }
    public static List<User> UsersOverview()
    {
        return db.Users.ToList();
    }
}