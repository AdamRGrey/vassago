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
    private static List<Behavior> behaviors { get; set; } = new List<Behavior>();
    internal Behaver()
    {
        var subtypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(domainAssembly => domainAssembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(Behavior)) && !type.IsAbstract)
            .ToList();

        foreach (var subtype in subtypes)
        {
            behaviors.Add((Behavior)Activator.CreateInstance(subtype));
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
        var permissions = new PermissionSettings(); //TODO: get combined permissions for author and channel
        var contentWithoutMention = message.Content;

        foreach (var behavior in behaviors)
        {
            if (behavior.ShouldAct(permissions, message))
            {
                behavior.ActOn(permissions, message);
                message.ActedOn = true;
            }
        }
        if (message.ActedOn == false && message.MentionsMe && contentWithoutMention.Contains('?'))
        {
            Console.WriteLine("providing bullshit nonanswer / admitting uselessness");
            var responses = new List<string>(){
                                @"Well, that's a great question, and there are certainly many different possible answers. Ultimately, the decision will depend on a variety of factors, including your personal interests and goals, as well as any practical considerations (like the economy). I encourage you to do your research, speak with experts and educators, and explore your options before making a decision that's right for you.",
                                @"┐(ﾟ ～ﾟ )┌",@"¯\_(ツ)_/¯",@"╮ (. ❛ ᴗ ❛.) ╭", @"╮(╯ _╰ )╭"
                            };
            await message.Channel.SendMessage(responses[Shared.r.Next(responses.Count)]);
            message.ActedOn = true;
        }
        if (message.ActedOn)
        {
            Shared.dbContext.SaveChanges();
        }
        return message.ActedOn;
    }
}
#pragma warning restore 4014 //the "async not awaited" error