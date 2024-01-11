namespace vassago.Behavior;
#pragma warning disable 4014 //the "not awaited" error
using vassago.Models;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Behaver
{
    private List<Account> SelfAccounts { get; set; } = new List<Account>();
    private User SelfUser { get; set; }
    public static List<Behavior> Behaviors { get; private set; } = new List<Behavior>();
    internal Behaver()
    {
        var subtypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(domainAssembly => domainAssembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(Behavior)) && !type.IsAbstract &&
                type.GetCustomAttributes(typeof(StaticPlzAttribute),false)?.Any() == true)
            .ToList();

        foreach (var subtype in subtypes)
        {
            Behaviors.Add((Behavior)Activator.CreateInstance(subtype));
        }
    }
    static Behaver() { }

    private static readonly Behaver _instance = new Behaver();

    public static Behaver Instance
    {
        get { return _instance; }
    }

    public async Task<bool> ActOn(Message message)
    {
        foreach (var behavior in Behaviors)
        {
            if (behavior.ShouldAct(message))
            {
                behavior.ActOn(message);
                message.ActedOn = true;
                Console.WriteLine("acted on, moving forward");
            }
        }
        if (message.ActedOn == false && message.MentionsMe && message.Content.Contains('?') && !Behaver.Instance.SelfAccounts.Any(acc => acc.Id == message.Author.Id))
        {
            Console.WriteLine("providing bullshit nonanswer / admitting uselessness");
            var responses = new List<string>(){
                                @"Well, that's a great question, and there are certainly many different possible answers. Ultimately, the decision will depend on a variety of factors, including your personal interests and goals, as well as any practical considerations (like the economy). I encourage you to do your research, speak with experts and educators, and explore your options before making a decision that's right for you.",
                                @"┐(ﾟ ～ﾟ )┌", @"¯\_(ツ)_/¯", @"╮ (. ❛ ᴗ ❛.) ╭", @"╮(╯ _╰ )╭"
                            };
            await message.Channel.SendMessage(responses[Shared.r.Next(responses.Count)]);
            message.ActedOn = true;
        }
        return message.ActedOn;
    }

    internal bool IsSelf(Guid AccountId)
    {
        var db = new ChattingContext();
        var acc = db.Accounts.Find(AccountId);

        return SelfAccounts.Any(acc => acc.Id == AccountId);
    }

    public void MarkSelf(Account selfAccount)
    {
        var db = new ChattingContext();
        if(SelfUser == null)
        {
            SelfUser = selfAccount.IsUser;
        }
        else if (SelfUser != selfAccount.IsUser)
        {
            CollapseUsers(SelfUser, selfAccount.IsUser, db);
        }
        SelfAccounts = db.Accounts.Where(a => a.IsUser == SelfUser).ToList();
    }

    public bool CollapseUsers(User primary, User secondary, ChattingContext db)
    {
        Console.WriteLine($"{secondary.Id} is being consumed into {primary.Id}");
        primary.Accounts.AddRange(secondary.Accounts);
        foreach(var a in secondary.Accounts)
        {
            a.IsUser = primary;
        }
        secondary.Accounts.Clear();
        Console.WriteLine("accounts transferred");
        try
        {
            db.SaveChangesAsync().Wait();
        }
        catch(Exception e)
        {
            Console.WriteLine("First save exception.");
            Console.Error.WriteLine(e);
            return false;
        }
        Console.WriteLine("saved");


        db.Users.Remove(secondary);
        Console.WriteLine("old account cleaned up");
        try
        {
            db.SaveChangesAsync().Wait();
        }
        catch(Exception e)
        {
            Console.WriteLine("Second save exception.");
            Console.Error.WriteLine(e);
            return false;
        }
        Console.WriteLine("saved, again, separately");
        return true;
    }
}
#pragma warning restore 4014 //the "async not awaited" error
