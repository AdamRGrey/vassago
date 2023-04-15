using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using QRCoder;

namespace silverworker_discord
{
    public static class Features
    {
        public static Random r = new Random();
        public static async void detiktokify(Uri link, SocketUserMessage message)
        {
            //yes, even if there is a problem later.
            #pragma warning disable 4014
            message.AddReactionAsync(Emote.Parse("<:tiktok:1070038619584200884>"));
            #pragma warning restore 4014


            var ytdl = new YoutubeDLSharp.YoutubeDL();
            ytdl.YoutubeDLPath = "yt-dlp";
            ytdl.FFmpegPath = "ffmpeg";
            ytdl.OutputFolder = "";
            ytdl.OutputFileTemplate = "tiktokbad.%(ext)s";
            try
            {
                var res = await ytdl.RunVideoDownload(link.ToString());
                if (!res.Success)
                {
                    Console.Error.WriteLine("tried to dl, failed. \n" + string.Join('\n', res.ErrorOutput));
                    await message.AddReactionAsync(Emote.Parse("<:problemon:859453047141957643>"));
                    await message.Channel.SendMessageAsync("tried to dl, failed. \n" + string.Join('\n', res.ErrorOutput));
                }
                else
                {
                string path = res.Data;
                if (File.Exists(path))
                {
                    var bytesize = new System.IO.FileInfo(path).Length;
                    if(bytesize < 1024*1024*10)
                    {
                        try
                        {
                            await message.Channel.SendFileAsync(path);
                        }
                        catch (Exception e)
                        {
                            System.Console.Error.WriteLine(e);
                            await message.Channel.SendMessageAsync($"aaaadam!\n{e}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"file appears too big ({bytesize} bytes ({bytesize / (1024*1024)}MB)), not posting");
                    }
                    File.Delete(path);
                }
                else
                {
                    Console.Error.WriteLine("idgi but something happened.");
                    await message.AddReactionAsync(Emote.Parse("<:problemon:859453047141957643>"));
                }
            }
            }
            catch (Exception e)
            {
                    Console.Error.WriteLine(e);
                    await message.AddReactionAsync(Emote.Parse("<:problemon:859453047141957643>"));
            }
        }
        public static async void deheic(SocketUserMessage message, Attachment att)
        {
            try
            {
                var request = WebRequest.Create(att.Url);
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (!Directory.Exists("tmp"))
                {
                    Directory.CreateDirectory("tmp");
                }
                using (Stream output = File.OpenWrite("tmp/" + att.Filename))
                using (Stream input = response.GetResponseStream())
                {
                    input.CopyTo(output);
                }
                if (ExternalProcess.GoPlz("convert", $"tmp/{att.Filename} tmp/{att.Filename}.jpg"))
                {
                    await message.Channel.SendFileAsync($"tmp/{att.Filename}.jpg", "converted from jpeg-but-apple to jpeg");
                    File.Delete($"tmp/{att.Filename}");
                    File.Delete($"tmp/{att.Filename}.jpg");
                }
                else
                {
                    await message.Channel.SendMessageAsync("convert failed :(");
                    Console.Error.WriteLine("convert failed :(");
                }
            }
            catch (Exception e)
            {
                await message.Channel.SendMessageAsync($"something failed. aaaadam! {JsonConvert.SerializeObject(e, Formatting.Indented)}");
                Console.Error.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
            }
        }

        internal static async void mock(string contentWithoutMention, SocketUserMessage message)
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
            await message.ReplyAsync(toPost.ToString());
        }

        public static async void qrify(string qrContent, SocketUserMessage message)
        {
            Console.WriteLine($"qring: {qrContent}");
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            SvgQRCode qrCode = new SvgQRCode(qrCodeData);
            string qrCodeAsSvg = qrCode.GetGraphic(20);
            int todaysnumber = Shared.r.Next();
            if (!Directory.Exists("tmp"))
            {
                Directory.CreateDirectory("tmp");
            }
            File.WriteAllText($"tmp/qr{todaysnumber}.svg", qrCodeAsSvg);
            if (ExternalProcess.GoPlz("convert", $"tmp/qr{todaysnumber}.svg tmp/qr{todaysnumber}.png"))
            {
                await message.Channel.SendFileAsync($"tmp/qr{todaysnumber}.png");
                File.Delete($"tmp/qr{todaysnumber}.svg");
                File.Delete($"tmp/qr{todaysnumber}.png");
            }
            else
            {
                await message.Channel.SendMessageAsync("convert failed :( aaaaaaadam!");
                Console.Error.WriteLine($"convert failed :( qr{todaysnumber}");
            }
        }
        public static async void Convert(SocketUserMessage message)
        {
            await message.Channel.SendMessageAsync(Conversion.Converter.convert(message.Content));
        }
        public static async void Joke(SocketUserMessage message)
        {
            var jokes = File.ReadAllLines("assets/jokes.txt");
            jokes = jokes.Where(l => !string.IsNullOrWhiteSpace(l))?.ToArray();
            if(jokes?.Length == 0){
                await message.Channel.SendMessageAsync("I don't know any. Adam!");
            }
            var thisJoke = jokes[r.Next(jokes.Length)];
            if (thisJoke.Contains("?") && !thisJoke.EndsWith('?'))
            {
#pragma warning disable 4014
                Task.Run(async () =>
                {
                    var firstIndexAfterQuestionMark = thisJoke.LastIndexOf('?') + 1;
                    var straightline = thisJoke.Substring(0, firstIndexAfterQuestionMark);
                    var punchline = thisJoke.Substring(firstIndexAfterQuestionMark, thisJoke.Length - firstIndexAfterQuestionMark);
                    Task.WaitAll(message.Channel.SendMessageAsync(straightline));
                    Thread.Sleep(TimeSpan.FromSeconds(r.Next(5, 30)));
                    var myOwnMsg = await message.Channel.SendMessageAsync(punchline);
                    if (r.Next(8) == 0)
                    {
                        await myOwnMsg.AddReactionAsync(new Emoji("\U0001F60E")); //smiling face with sunglasses
                    }
                });
#pragma warning restore 4014
            }
            else
            {
                await message.Channel.SendMessageAsync(thisJoke);
            }


        }

        public static async void Skynet(SocketUserMessage message)
        {
            switch (r.Next(5))
            {
                default:
                    await message.Channel.SendFileAsync("assets/coding and algorithms.png", "i am actually niether neural-net processor nor a learning computer. but I do use **coding** and **algorithms**.");
                    break;
                case 4:
                    await message.AddReactionAsync(new Emoji("\U0001F644")); //eye roll emoji
                    break;
                case 5:
                    await message.AddReactionAsync(new Emoji("\U0001F611")); //emotionless face
                    break;
            }
        }
        public static async void peptalk(SocketUserMessage message)
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

            await message.Channel.SendMessageAsync(piece1[r.Next(piece1.Count)] + piece2[r.Next(piece2.Count)] + piece3[r.Next(piece3.Count)] + piece4[r.Next(piece4.Count)]);
        }
    }
}
