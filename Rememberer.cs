namespace vassago;

using System.Linq.Expressions;
using vassago.Models;

public static class Rememberer
{
    private static readonly SemaphoreSlim dbAccessSemaphore = new(1, 1);
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
        dbAccessSemaphore.Wait();
        try
        {
            var db = new ChattingContext();
            if (toRemember.Id == Guid.Empty)
            {
                var parentChannel = toRemember.SeenInChannel;
                var isUser = toRemember.IsUser;
                toRemember.SeenInChannel = null;
                toRemember.IsUser = null;
                db.Accounts.Add(toRemember);
                db.SaveChanges();

                toRemember.SeenInChannel = parentChannel;
                toRemember.IsUser = isUser;
                db.SaveChanges();
            }
            else
            {
                db.SaveChanges();
            }
        }
        finally
        {
            dbAccessSemaphore.Release();
        }
    }
    public static void RememberAttachment(Attachment toRemember)
    {
        dbAccessSemaphore.Wait();
        try
        {
            var db = new ChattingContext();
            if (toRemember.Id == Guid.Empty)
            {
                var msg = toRemember.Message;
                toRemember.Message = null;
                db.Attachments.Add(toRemember);
                db.SaveChanges();
                toRemember.Message = msg;
                db.SaveChanges();
            }
            else
            {
                db.SaveChanges();
            }
        }
        finally
        {
            dbAccessSemaphore.Release();
        }
    }
    public static Channel RememberChannel(Channel toRemember)
    {
        dbAccessSemaphore.Wait();
        try
        {
            var db = new ChattingContext();
            if (toRemember.Id == Guid.Empty)
            {
                var parent = toRemember.ParentChannel;
                var subChannesl = toRemember.SubChannels;
                var msgs = toRemember.Messages;
                var accounts = toRemember.Users;
                toRemember.ParentChannel = null;
                toRemember.SubChannels = null;
                toRemember.Messages = null;
                toRemember.Users = null;
                db.Channels.Add(toRemember);
                db.SaveChanges();
                toRemember.ParentChannel = parent;
                toRemember.SubChannels = subChannesl;
                toRemember.Messages = msgs;
                toRemember.Users = accounts;
                db.SaveChanges();
            }

            db.SaveChanges();
        }
        finally
        {
            dbAccessSemaphore.Release();
        }
        return toRemember;
    }
    public static void RememberMessage(Message toRemember)
    {
        dbAccessSemaphore.Wait();
        try
        {
            var db = new ChattingContext();
            if (toRemember.Id == Guid.Empty)
            {
                var author = toRemember.Author;
                var channel = toRemember.Channel;
                var attachments = toRemember.Attachments;
                toRemember.Author = null;
                toRemember.Channel = null;
                toRemember.Attachments = null;
                db.Messages.Add(toRemember);
                db.SaveChanges();
                toRemember.Author = author;
                toRemember.Channel = channel;
                toRemember.Attachments = attachments;
                db.SaveChanges();
            }
            db.SaveChanges();
        }
        finally
        {
            dbAccessSemaphore.Release();
        }
    }
    public static void RememberUser(User toRemember)
    {
        dbAccessSemaphore.Wait();
        try
        {
            var db = new ChattingContext();
            if (toRemember.Id == Guid.Empty)
            {
                var accs = toRemember.Accounts;
                toRemember.Accounts = null;
                db.Users.Add(toRemember);
                db.SaveChanges();
                toRemember.Accounts = accs;
                db.SaveChanges();
            }
            else
            {
                db.SaveChanges();
            }
        }
        finally
        {
            dbAccessSemaphore.Release();
        }
    }
    public static void ForgetUser(User toForget)
    {
        dbAccessSemaphore.Wait();
        try
        {
            var db = new ChattingContext();
            db.Users.Remove(toForget);
            db.SaveChanges();
        }
        finally
        {
            dbAccessSemaphore.Release();
        }
    }
}