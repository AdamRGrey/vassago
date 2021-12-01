using System;
using System.IO;
using System.Net;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using QRCoder;

namespace silverworker_discord
{
    public static class Features
    {
        public static async void detiktokify(Uri link, SocketUserMessage message)
        {
            var ytdl = new YoutubeDLSharp.YoutubeDL();
            ytdl.YoutubeDLPath = "youtube-dl";
            ytdl.FFmpegPath = "ffmpeg";
            ytdl.OutputFolder = "";
            ytdl.OutputFileTemplate = "tiktokbad.%(ext)s";
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
                    try
                    {
                        await message.Channel.SendFileAsync(path);
                    }
                    catch (Exception e)
                    {
                        await message.Channel.SendMessageAsync($"aaaadam!\n{JsonConvert.SerializeObject(e)}");
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
        public static async void deheic(SocketUserMessage message, Attachment att)
        {
            try
            {
                var request = WebRequest.Create(att.Url);
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if(!Directory.Exists("tmp"))
                {
                    Directory.CreateDirectory("tmp");
                }
                using (Stream output = File.OpenWrite("tmp/" + att.Filename))
                using (Stream input = response.GetResponseStream())
                {
                    input.CopyTo(output);
                }
                if(ExternalProcess.GoPlz("convert", $"tmp/{att.Filename} tmp/{att.Filename}.jpg"))
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
        public static async void qrify(string qrContent, SocketUserMessage message)
        {
            Console.WriteLine($"qring: {qrContent}");
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            SvgQRCode qrCode = new SvgQRCode(qrCodeData);
            string qrCodeAsSvg = qrCode.GetGraphic(20);
            int todaysnumber = Shared.r.Next();
            if(!Directory.Exists("tmp"))
            {
                Directory.CreateDirectory("tmp");
            }
            File.WriteAllText($"tmp/qr{todaysnumber}.svg", qrCodeAsSvg);
            if(ExternalProcess.GoPlz("convert", $"tmp/qr{todaysnumber}.svg tmp/qr{todaysnumber}.png"))
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
    }
}