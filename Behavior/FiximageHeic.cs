namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using vassago.Models;

[StaticPlz]
public class FiximageHeic : Behavior
{
    public override string Name => "deheic";

    public override string Trigger => "post an heic image";

    public override string Description => "convert heic images to jpg";

    private List<Attachment> heics = new List<Attachment>();
    public override bool ShouldAct(Message message)
    {
        if(Behaver.Instance.IsSelf(message.Author.Id))
            return false;

        if (message.Attachments?.Count() > 0)
        {
            foreach (var att in message.Attachments)
            {
                if (att.Filename?.EndsWith(".heic") == true)
                {
                    heics.Add(att);
                }
            }
        }
        return heics.Any();
    }

    public override async Task<bool> ActOn(Message message)
    {
        if (!Directory.Exists("tmp"))
        {
            Directory.CreateDirectory("tmp");
        }
        var conversions = new List<Task<bool>>();
        foreach (var att in heics)
        {
            conversions.Add(actualDeheic(att, message));
        }
        Task.WaitAll(conversions.ToArray());
        await message.React("\U0001F34F");
        return true;
    }

    private async Task<bool> actualDeheic(Attachment att, Message message)
    {
        try
        {
            var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;
            using (Stream output = File.OpenWrite("tmp/" + att.Filename))
            {
                (await Shared.HttpClient.GetAsync(att.Source))
                    .Content.CopyTo(output, null, token);
            }
            if (ExternalProcess.GoPlz("convert", $"tmp/{att.Filename} tmp/{att.Filename}.jpg"))
            {
                await message.Channel.SendFile($"tmp/{att.Filename}.jpg", "converted from jpeg-but-apple to jpeg");
                File.Delete($"tmp/{att.Filename}");
                File.Delete($"tmp/{att.Filename}.jpg");
            }
            else
            {
                await message.Channel.SendMessage("convert failed :(");
                Console.Error.WriteLine("convert failed :(");
            }
        }
        catch (Exception e)
        {
            await message.Channel.SendMessage($"something failed. aaaadam! {JsonConvert.SerializeObject(e, Formatting.Indented)}");
            Console.Error.WriteLine(JsonConvert.SerializeObject(e, Formatting.Indented));
            return false;
        }
        return true;
    }
}
