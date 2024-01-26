namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using vassago.Models;

[StaticPlz]
public class Detiktokify : Behavior
{
    public override string Name { get => "Detiktokify"; }
    public override string Trigger { get => "post a link below vm.tiktok.com"; }
    public override string Description { get => "re-host tiktok content"; }

    private List<Uri> tiktokLinks = new List<Uri>();
    private YoutubeDLSharp.YoutubeDL ytdl;
    public Detiktokify()
    {
        ytdl = new YoutubeDLSharp.YoutubeDL();
        ytdl.YoutubeDLPath = "yt-dlp";
        ytdl.FFmpegPath = "ffmpeg";
        ytdl.OutputFolder = "";
        ytdl.OutputFileTemplate = "tiktokbad.%(ext)s";
    }
    public override bool ShouldAct(Message message)
    {

        if(Behaver.Instance.IsSelf(message.Author.Id))
            return false;

        if(message.Channel.EffectivePermissions.MaxAttachmentBytes == 0)
            return false;

        var wordLikes = message.Content.Split(' ', StringSplitOptions.TrimEntries);
        var possibleLinks = wordLikes?.Where(wl => Uri.IsWellFormedUriString(wl, UriKind.Absolute)).Select(wl => new Uri(wl));
        if (possibleLinks != null && possibleLinks.Count() > 0)
        {
            foreach (var link in possibleLinks)
            {
                if (link.Host.EndsWith(".tiktok.com"))
                {
                    tiktokLinks.Add(link);
                }
            }
        }
        if(tiktokLinks.Any()){
            Console.WriteLine($"Should Act on message id {message.ExternalId}; with content {message.Content}");
        }
        return tiktokLinks.Any();
    }
    public override async Task<bool> ActOn(Message message)
    {
        foreach(var link in tiktokLinks)
        {
            tiktokLinks.Remove(link);
            try
            {
                Console.WriteLine($"detiktokifying {link}");
                #pragma warning disable 4014
                //await message.React("<:tiktok:1070038619584200884>");
                #pragma warning restore 4014

                var res = await ytdl.RunVideoDownload(link.ToString());
                if (!res.Success)
                {
                    Console.Error.WriteLine("tried to dl, failed. \n" + string.Join('\n', res.ErrorOutput));
                    await message.React("problemon");
                    await message.Channel.SendMessage("tried to dl, failed. \n");
                }
                else
                {
                    string path = res.Data;
                    if (File.Exists(path))
                    {
                        ulong bytesize = (ulong)((new System.IO.FileInfo(path)).Length);
                        if (bytesize < message.Channel.EffectivePermissions.MaxAttachmentBytes - 256)
                        {
                            try
                            {
                                await message.Channel.SendFile(path, null);
                            }
                            catch (Exception e)
                            {
                                System.Console.Error.WriteLine(e);
                                await message.Channel.SendMessage($"aaaadam!\n{e}");
                            }
                        }
                        else
                        {
                            message.ActedOn = true;
                            Console.WriteLine($"file appears too big ({bytesize} bytes ({bytesize / (1024 * 1024)}MB)), not posting");
                        }
                        File.Delete(path);
                    }
                    else
                    {
                        Console.Error.WriteLine("idgi but something happened.");
                        await message.React("problemon");
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                await message.React("problemon");
                return false;
            }
        }
        return true;
    }
}
