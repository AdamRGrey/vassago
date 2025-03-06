namespace vassago;
#pragma warning disable 4014 //the "not awaited" error
using vassago.Behavior;
using vassago.Models;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using vassago.ProtocolInterfaces.DiscordInterface;

public class Behaver
{
    private List<Account> SelfAccounts { get; set; } = new List<Account>();
    private User SelfUser { get; set; }
    public static List<vassago.Behavior.Behavior> Behaviors { get; private set; } = new List<vassago.Behavior.Behavior>();
    internal Behaver()
    {
        var subtypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(domainAssembly => domainAssembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(vassago.Behavior.Behavior)) && !type.IsAbstract &&
                type.GetCustomAttributes(typeof(StaticPlzAttribute),false)?.Any() == true)
            .ToList();

        foreach (var subtype in subtypes)
        {
            Behaviors.Add((vassago.Behavior.Behavior)Activator.CreateInstance(subtype));
        }
    }
    static Behaver() { }

    private static readonly Behaver _instance = new Behaver();

    //TODO: you know why I didn't make this a static class? lifecycle issues with the dbcontext. but now that we don't have a stored instance, 
    //no need to have a... *checks over shoulder*... *whispers*: singleton
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
        if(SelfUser == null)
        {
            SelfUser = selfAccount.IsUser;
        }
        else if (SelfUser != selfAccount.IsUser)
        {
            CollapseUsers(SelfUser, selfAccount.IsUser);
        }
        SelfAccounts = Rememberer.SearchAccounts(a => a.IsUser == SelfUser);
        Rememberer.RememberAccount(selfAccount);
    }

    public bool CollapseUsers(User primary, User secondary)
    {
        if(primary.Accounts == null)
            primary.Accounts = new List<Account>();
        if(secondary.Accounts != null)
            primary.Accounts.AddRange(secondary.Accounts);
        foreach(var a in secondary.Accounts)
        {
            a.IsUser = primary;
        }
        secondary.Accounts.Clear();
        Rememberer.ForgetUser(secondary);
        Rememberer.RememberUser(primary);
        return true;
    }
}
#pragma warning restore 4014 //the "async not awaited" error
