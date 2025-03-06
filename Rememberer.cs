namespace vassago;

using System.Linq.Expressions;
using vassago.Models;

public static class Rememberer
{
    private static readonly ChattingContext db = new();
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
        db.Update(toRemember);
        db.SaveChanges();
    }
    public static void RememberAttachment(Attachment toRemember)
    {
        db.Update(toRemember);

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
        db.Update(toRemember);

        db.SaveChanges();
    }
    public static void RememberUser(User toRemember)
    {
        db.Users.Update(toRemember);

        db.SaveChanges();
    }
    public static void ForgetUser(User toForget)
    {
        db.Users.Remove(toForget);

        db.SaveChanges();
    }
}