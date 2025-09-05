namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using vassago.Models;
using QRCoder;

[StaticPlz]
public class QRify : Behavior
{
    public override string Name => "qr-ify";

    public override string Trigger => "!qrplz";

    public override string Description => "generate text QR codes";

    public override bool ShouldAct(Message message, List<UAC> matchedUACs)
    {
        if (message.Channel.EffectivePermissions.MaxAttachmentBytes < 1024)
            return false;
        return base.ShouldAct(message, matchedUACs);
    }

    public override async Task<bool> ActOn(Message message)
    {
        var qrContent = message.Content.Substring($"{Trigger} ".Length + message.Content.IndexOf(Trigger));
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
        if ((new ExternalProcess()).GoPlz("convert", $"tmp/qr{todaysnumber}.svg tmp/qr{todaysnumber}.png"))
        {
            if (message.Channel.EffectivePermissions.MaxAttachmentBytes >= (ulong)(new System.IO.FileInfo($"tmp/qr{todaysnumber}.png").Length))
                Behaver.Instance.SendFile(message.Channel.Id, $"tmp/qr{todaysnumber}.png", null);
            else
                Behaver.Instance.SendMessage(message.Channel.Id, $"resulting qr image 2 big 4 here ({(ulong)(new System.IO.FileInfo($"tmp / qr{ todaysnumber}.png").Length)} / {message.Channel.EffectivePermissions.MaxAttachmentBytes})");
            File.Delete($"tmp/qr{todaysnumber}.svg");
            File.Delete($"tmp/qr{todaysnumber}.png");
        }
        else
        {
            Behaver.Instance.SendMessage(message.Channel.Id, "convert failed :( aaaaaaadam!");
            Console.Error.WriteLine($"convert failed :( qr{todaysnumber}");
            return false;
        }
        return true;
    }
}
