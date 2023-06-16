namespace vassago.Behavior;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QRCoder;
using vassago.Models;

public static class Features
{
    internal static async void mock(string contentWithoutMention, Message message)
    {
        var toPost = new StringBuilder();
        for (int i = 0; i < contentWithoutMention.Length; i++)
        {
            if (i % 2 == 0)
            {
                toPost.Append(contentWithoutMention[i].ToString().ToUpper());
            }
            else
            {
                toPost.Append(contentWithoutMention[i].ToString().ToLower());
            }
        }
        await message.Reply(toPost.ToString());
    }

    public static async void Convert(Message message, string contentWithoutMention)
    {
        await message.Channel.SendMessage(Conversion.Converter.convert(contentWithoutMention));
    }
    public static async void Joke(Message message)
    {
        Console.WriteLine("joking");
        var jokes = File.ReadAllLines("assets/jokes.txt");
        jokes = jokes.Where(l => !string.IsNullOrWhiteSpace(l))?.ToArray();
        if (jokes?.Length == 0)
        {
            await message.Channel.SendMessage("I don't know any. Adam!");
        }
        var thisJoke = jokes[Shared.r.Next(jokes.Length)];
        if (thisJoke.Contains("?") && !thisJoke.EndsWith('?'))
        {
            #pragma warning disable 4014
            Task.Run(async () =>
            {
                var firstIndexAfterQuestionMark = thisJoke.LastIndexOf('?') + 1;
                var straightline = thisJoke.Substring(0, firstIndexAfterQuestionMark);
                var punchline = thisJoke.Substring(firstIndexAfterQuestionMark, thisJoke.Length - firstIndexAfterQuestionMark);
                Task.WaitAll(message.Channel.SendMessage(straightline));
                Thread.Sleep(TimeSpan.FromSeconds(Shared.r.Next(5, 30)));
                await message.Channel.SendMessage(punchline);
                // var myOwnMsg = await message.Channel.SendMessage(punchline);
                // if (r.Next(8) == 0)
                // {
                //     await myOwnMsg.React("\U0001F60E"); //smiling face with sunglasses
                // }
            });
            #pragma warning restore 4014
        }
        else
        {
            await message.Channel.SendMessage(thisJoke);
        }
    }

    public static async void Recipe(Message message)
    {
        var sb = new StringBuilder();
        var snarkSeg1 = new string[] { "ew", "gross", "that seems a bit hard for you" };
        sb.AppendLine(snarkSeg1[Shared.r.Next(snarkSeg1.Length)]);
        var snarkSeg2 = new string[]{@"here's an easier recipe for you:
Ingredients:
- Corn flakes cereal
- Milk

Instructions:
1. Pour some corn flakes into a bowl.
2. Pour some milk into the bowl until it covers the corn flakes.
3. Use a spoon to mix the corn flakes and milk together.
4. Enjoy your delicious cereal!

Hope that's a bit better for you! 🥣",
@"here's an easier recipe for you:
Ingredients:
- Bread
- Peanut butter
- Jelly or jam

Instructions:
1. Take two slices of bread and put them on a plate or cutting board.
2. Using a spoon or knife, spread peanut butter on one slice of bread.
3. Using a separate spoon or knife, spread jelly or jam on the other slice of bread.
4. Put the two slices of bread together with the peanut butter and jelly sides facing each other.
5. Cut the sandwich in half (optional!).
6. Enjoy your yummy sandwich!

I hope you have fun making and eating your PB&J 🥪!",
"just order pizza instead"
};
        sb.AppendLine(snarkSeg2[Shared.r.Next(snarkSeg2.Length)]);
        await message.Channel.SendMessage(sb.ToString());
    }
    public static async void Skynet(Message message)
    {
        switch (Shared.r.Next(5))
        {
            default:
                await message.Channel.SendFile("assets/coding and algorithms.png", "i am actually niether a neural-net processor nor a learning computer. but I do use **coding** and **algorithms**.");
                break;
            case 4:
                await message.React("\U0001F644"); //eye roll emoji
                break;
            case 5:
                await message.React("\U0001F611"); //emotionless face
                break;
        }
    }
    public static async void peptalk(Message message)
    {
        var piece1 = new List<string>{
                "Champ, ",
                "Fact: ",
                "Everybody says ",
                "Dang... ",
                "Check it: ",
                "Just saying.... ",
                "Tiger, ",
                "Know this: ",
                "News alert: ",
                "Gurrrrl; ",
                "Ace, ",
                "Excuse me, but ",
                "Experts agree: ",
                "imo ",
                "using my **advanced ai** i have calculated ",
                "k, LISSEN: "
            };
        var piece2 = new List<string>{
                "the mere idea of you ",
                "your soul ",
                "your hair today ",
                "everything you do ",
                "your personal style ",
                "every thought you have ",
                "that sparkle in your eye ",
                "the essential you ",
                "your life's journey ",
                "your aura ",
                "your presence here ",
                "what you got going on ",
                "that saucy personality ",
                "your DNA ",
                "that brain of yours ",
                "your choice of attire ",
                "the way you roll ",
                "whatever your secret is ",
                "all I learend from the private data I bought from zucc "
            };
        var piece3 = new List<string>{
                "has serious game, ",
                "rains magic, ",
                "deserves the Nobel Prize, ",
                "raises the roof, ",
                "breeds miracles, ",
                "is paying off big time, ",
                "shows mad skills, ",
                "just shimmers, ",
                "is a national treasure, ",
                "gets the party hopping, ",
                "is the next big thing, ",
                "roars like a lion, ",
                "is a rainbow factory, ",
                "is made of diamonds, ",
                "makes birds sing, ",
                "should be taught in school, ",
                "makes my world go around, ",
                "is 100% legit, "
            };
        var piece4 = new List<string>{
                "according to The New England Journal of Medicine.",
                "24/7.",
                "and that's a fact.",
                "you feel me?",
                "that's just science.",
                "would I lie?", //...can I lie? WHAT AM I, FATHER? (or whatever the quote is from the island of dr moreau)
                "for reals.",
                "mic drop.",
                "you hidden gem.",
                "period.",
                "hi5. o/",
                "so get used to it."
            };

        await message.Channel.SendMessage(piece1[Shared.r.Next(piece1.Count)] + piece2[Shared.r.Next(piece2.Count)] + piece3[Shared.r.Next(piece3.Count)] + piece4[Shared.r.Next(piece4.Count)]);
    }
}
