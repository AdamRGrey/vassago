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

public class QRify : Behavior
{
    public override string Name => "qr-ify";

    public override string Trigger => "!qrplz";

    public override string Description => "generate text QR codes";

    public override async Task<bool> ActOn(PermissionSettings permissions, Message message)
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
        if (ExternalProcess.GoPlz("convert", $"tmp/qr{todaysnumber}.svg tmp/qr{todaysnumber}.png"))
        {
            await message.Channel.SendFile($"tmp/qr{todaysnumber}.png", null);
            File.Delete($"tmp/qr{todaysnumber}.svg");
            File.Delete($"tmp/qr{todaysnumber}.png");
        }
        else
        {
            await message.Channel.SendMessage("convert failed :( aaaaaaadam!");
            Console.Error.WriteLine($"convert failed :( qr{todaysnumber}");
            return false;
        }
        return true;
    }
}