namespace vassago;

using System.Linq.Expressions;
using vassago.Models;

public static class Rememberer
{
    public static Account SearchAccount(Expression<Func<Account, bool>> predicate)
    {
        return (new ChattingContext()).Accounts.FirstOrDefault(predicate);
    }
    public static List<Account> SearchAccounts(Expression<Func<Account, bool>> predicate)
    {
        return (new ChattingContext()).Accounts.Where(predicate).ToList();
    }
    public static Attachment SearchAttachment(Expression<Func<Attachment, bool>> predicate)
    {
        return (new ChattingContext()).Attachments.FirstOrDefault(predicate);
    }
    public static Channel SearchChannel(Expression<Func<Channel, bool>> predicate)
    {
        return (new ChattingContext()).Channels.FirstOrDefault(predicate);
    }
    public static Message SearchMessage(Expression<Func<Message, bool>> predicate)
    {
        return (new ChattingContext()).Messages.FirstOrDefault(predicate);
    }
    public static User SearchUser(Expression<Func<User, bool>> predicate)
    {
        return (new ChattingContext()).Users.FirstOrDefault(predicate);
    }
    public static void RememberAccount(Account toRemember)
    {
        var db = new ChattingContext();
        if (toRemember.Id == Guid.Empty)
            db.Accounts.Add(toRemember);

        db.SaveChanges();
    }
    public static void RememberAttachment(Attachment toRemember)
    {
        var db = new ChattingContext();
        if (toRemember.Id == Guid.Empty)
            db.Attachments.Add(toRemember);

        db.SaveChanges();
    }
    public static void RememberChannel(Channel toRemember)
    {
        var db = new ChattingContext();
        if (toRemember.Id == Guid.Empty)
            db.Channels.Add(toRemember);

        db.SaveChanges();
    }
    public static void RememberMessage(Message toRemember)
    {
        var db = new ChattingContext();
        if (toRemember.Id == Guid.Empty)
        {
            db.Messages.Add(toRemember);
        }
        db.SaveChanges();
    }
    public static void RememberUser(User toRemember)
    {
        var db = new ChattingContext();
        if (toRemember.Id == Guid.Empty)
            db.Users.Add(toRemember);

        db.SaveChanges();
    }
    public static void ForgetUser(User toForget)
    {
        var db = new ChattingContext();
        db.Users.Remove(toForget);
        db.SaveChanges();
    }
}