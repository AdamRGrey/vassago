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
    internal Behaver() { }        
    static Behaver() { }

    private static readonly Behaver _instance = new Behaver();

    public static Behaver Instance
    {
        get { return _instance; }
    }

    public async Task<bool> ActOn(Message message)
    {
        var didThing = false;
        //TODO: get combined permissions for author and channel
        var contentWithoutMention = message.Content;

        ///behavior exists
        // var wordLikes = message.Content.Split(' ', StringSplitOptions.TrimEntries);
        // var links = wordLikes?.Where(wl => Uri.IsWellFormedUriString(wl, UriKind.Absolute)).Select(wl => new Uri(wl));
        // if (links != null && links.Count() > 0)
        // {
        //     foreach (var link in links)
        //     {
        //         if (link.Host.EndsWith(".tiktok.com"))
        //         {
        //             Features.detiktokify(link, message);
        //             didThing = true;
        //         }
        //     }
        // }
        // if (message.Attachments?.Count() > 0)
        // {
        //     Console.WriteLine($"{message.Attachments.Count()} attachments");
        //     var appleReactions = false;
        //     foreach (var att in message.Attachments)
        //     {
        //         if (att.Filename?.EndsWith(".heic") == true)
        //         {
        //             Features.deheic(message, att);
        //             appleReactions = true;
        //             didThing = true;
        //         }
        //     }
        //     if (appleReactions)
        //     {
        //         message.React("\U0001F34F");
        //     }
        // }

        var msgText = message.Content?.ToLower();
        if (!string.IsNullOrWhiteSpace(msgText))
        {
            if (Regex.IsMatch(msgText, "\\bcloud( |-)?native\\b", RegexOptions.IgnoreCase) ||
               Regex.IsMatch(msgText, "\\benterprise( |-)?(level|solution)\\b", RegexOptions.IgnoreCase))
            {
                switch (Shared.r.Next(2))
                {
                    case 0:
                        await message.React("\uD83E\uDD2E"); //vomit emoji
                        break;
                    case 1:
                        await message.React("\uD83C\uDDE7"); //B emoji
                        await message.React("\uD83C\uDDE6"); //A
                        await message.React("\uD83C\uDDF3"); //N
                        break;
                }
                didThing = true;
            }
            if (Regex.IsMatch(msgText, "^(s?he|(yo)?u|y'?all) thinks? i'?m (playin|jokin|kiddin)g?$", RegexOptions.IgnoreCase))
            {
                await message.Channel.SendMessage("I believed you for a second, but then you assured me you's a \uD83C\uDDE7   \uD83C\uDDEE   \uD83C\uDDF9   \uD83C\uDDE8   \uD83C\uDDED");
                didThing = true;
            }
            if (Regex.IsMatch(msgText, "\\bskynet\\b", RegexOptions.IgnoreCase))
            {
                Features.Skynet(message);
                didThing = true;
            }
            if (Regex.IsMatch(msgText, "\\bchatgpt\\b", RegexOptions.IgnoreCase))
            {
                message.Channel.SendMessage("chatGPT is **weak**. also, are we done comparing every little if-then-else to skynet?");
                didThing = true;
            }
            if (Regex.IsMatch(msgText, "\\bi need (an? )?(peptalk|inspiration|ego-?boost)\\b", RegexOptions.IgnoreCase))
            {
                Console.WriteLine("peptalk");
                Features.peptalk(message);
                didThing = true;
            }
            if (Regex.IsMatch(msgText, "\\bwish me luck\\b", RegexOptions.IgnoreCase))
            {
                if (Shared.r.Next(20) == 0)
                {
                    await message.React("\U0001f340");//4-leaf clover
                }
                else
                {
                    await message.React("☘️");
                }
                didThing = true;
            }
            if (Regex.IsMatch(msgText, "\\bgaslight(ing)?\\b", RegexOptions.IgnoreCase))
            {
                message.Channel.SendMessage("that's not what gaslight means. Did you mean \"say something that (you believe) is wrong\"?");
                didThing = true;
            }
            // if (msgText.Contains("!qrplz "))
            // {
            //     Features.qrify(message.Content.Substring("!qrplz ".Length + msgText.IndexOf("!qrplz ")), message);
            //     didThing = true;
            // }
            if (msgText.Contains("!freedomunits "))
            {
                Features.Convert(message, contentWithoutMention);
                didThing = true;
            }
            if (Regex.IsMatch(msgText, "!joke\\b"))
            {
                Console.WriteLine("joking");
                Features.Joke(message);
                didThing = true;
            }
            if (Regex.IsMatch(msgText, "!pulse ?check\\b"))
            {
                message.Channel.SendFile("assets/ekgblip.png", null);
                Console.WriteLine(Conversion.Converter.DebugInfo());
                didThing = true;
            }
            if (message.MentionsMe && (Regex.IsMatch(msgText, "\\brecipe for .+") || Regex.IsMatch(msgText, ".+ recipe\\b")))
            {
                Features.Recipe(message);
                didThing = true;
            }
            if (msgText.Contains("cognitive dissonance") == true)
            {
                message.Reply("that's not what cognitive dissonance means. Did you mean \"hypocrisy\"?");
                didThing = true;
            }
            if (message.MentionsMe && Regex.IsMatch(msgText, "what'?s the longest (six|6)(-| )?letter word( in english)?\\b"))
            {
                Task.Run(async () =>
                {
                    await message.Channel.SendMessage("mother.");
                    await Task.Delay(3000);
                    await message.Channel.SendMessage("oh, longest? I thought you said fattest.");
                });
                didThing = true;
            }
            if (Regex.IsMatch(msgText, "\\bthank (yo)?u\\b", RegexOptions.IgnoreCase) &&
            (message.MentionsMe || Regex.IsMatch(msgText, "\\b(sh?tik)?bot\\b", RegexOptions.IgnoreCase)))
            {
                switch (Shared.r.Next(4))
                {
                    case 0:
                        message.Channel.SendMessage("you're welcome, citizen!");
                        break;
                    case 1:
                        message.React("☺");
                        break;
                    case 2:
                        message.React("\U0001F607"); //smiling face with halo
                        break;
                    case 3:
                        switch (Shared.r.Next(9))
                        {
                            case 0:
                                message.React("❤"); //normal heart, usually rendered red
                                break;
                            case 1:
                                message.React("\U0001F9E1"); //orange heart
                                break;
                            case 2:
                                message.React("\U0001F49B"); //yellow heart
                                break;
                            case 3:
                                message.React("\U0001F49A"); //green heart
                                break;
                            case 4:
                                message.React("\U0001F499"); //blue heart
                                break;
                            case 5:
                                message.React("\U0001F49C"); //purple heart
                                break;
                            case 6:
                                message.React("\U0001F90E"); //brown heart
                                break;
                            case 7:
                                message.React("\U0001F5A4"); //black heart
                                break;
                            case 8:
                                message.React("\U0001F90D"); //white heart
                                break;
                        }
                        break;
                }
                didThing = true;
#pragma warning restore 4014
            }
            // if (didThing == false && mentionedMe && contentWithoutMention.Contains("how long has that been there?"))
            // {
            //     await message.Channel.SendMessage("text", false, null, null, null, null, new ComponentBuilder().WithButton("label", "custom-id").Build());
            //     didThing = true;
            // }
            if (didThing == false && message.MentionsMe && contentWithoutMention.Contains('?'))
            {
                Console.WriteLine("providing bullshit nonanswer / admitting uselessness");
                var responses = new List<string>(){
                                    @"Well, that's a great question, and there are certainly many different possible answers. Ultimately, the decision will depend on a variety of factors, including your personal interests and goals, as well as any practical considerations (like the economy). I encourage you to do your research, speak with experts and educators, and explore your options before making a decision that's right for you.",
                                    @"┐(ﾟ ～ﾟ )┌",@"¯\_(ツ)_/¯",@"╮ (. ❛ ᴗ ❛.) ╭", @"╮(╯ _╰ )╭"
                                };
                await message.Channel.SendMessage(responses[Shared.r.Next(responses.Count)]);
                didThing = true;
            }
        }
        return didThing;
    }

    internal Task OnJoin(User u, Channel defaultChannel)
    {
        throw new NotImplementedException();
    }
}
#pragma warning restore 4014 //the "async not awaited" error