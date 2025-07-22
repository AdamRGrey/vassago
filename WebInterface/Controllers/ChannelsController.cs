using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.WebInterface.Models;

namespace vassago.WebInterface.Controllers;

public class ChannelsController() : Controller
{
    private static Rememberer r = Rememberer.Instance;
    public IActionResult Details(Guid id)
    {
        var channel = r.ChannelDetail(id);
        if (channel == null)
        {
            return Problem($"couldn't find channle {id}");
        }
        else
        {
            Console.WriteLine($"details.cshtml will have a channel; {channel}.");
        }
        var walker = channel;
        while (walker != null)
        {
            ViewData["breadcrumbs"] = $"<a href=\"{Url.ActionLink(action: "Details", controller: "Channels", values: new { id = walker.Id })}\">{walker.DisplayName}</a>/" +
                ViewData["breadcrumbs"];
            walker = walker.ParentChannel;
        }
        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append($"{{text: \"{channel.SubChannels?.Count}\", nodes: [");
        var first = true;
        foreach (var subChannel in channel.SubChannels)
        {
            if (!first)
            {
                sb.Append(',');
            }
            else
            {
                first = false;
            }
            sb.Append($"{{\"text\": \"<a href=\\\"{Url.ActionLink(action: "Details", controller: "Channels", values: new { id = subChannel.Id })}\\\">{subChannel.DisplayName}</a>\"}}");
        }
        sb.Append("]}]");

        ViewData.Add("subChannelsTree", sb.ToString());
        return View(
            new Tuple<Channel, Enumerations.LewdnessFilterLevel?, Enumerations.MeannessFilterLevel?>(
                channel,
                channel.ParentChannel?.EffectivePermissions.LewdnessFilterLevel,
                channel.ParentChannel?.EffectivePermissions.MeannessFilterLevel
            ));
    }

    [HttpPost]
    public IActionResult Submit(Channel channel)
    {
        var fromDb = r.ChannelDetail(channel.Id);
        fromDb.LewdnessFilterLevel = channel.LewdnessFilterLevel;
        fromDb.MeannessFilterLevel = channel.MeannessFilterLevel;
        r.RememberChannel(fromDb);
        return RedirectToAction("Details", "Channels", new { Id = fromDb.Id});
    }
    [HttpPost]
    public IActionResult UnlinkUAC(Guid ChannelId, Guid UACid)
    {
        var chan = r.ChannelDetail(ChannelId);
        var oldUAC = r.UACDetail(UACid);
        oldUAC.Channels.Remove(chan);
        r.RememberUAC(oldUAC);
        return RedirectToAction("Details", "Channels", new { Id = ChannelId});
    }
    [HttpPost]
    public IActionResult NewUAC(Guid Id)
    {
        Console.WriteLine($"new uac for channel {Id}");
        var chan = r.ChannelDetail(Id);
        Console.WriteLine($"channel null: {chan== null}");
        var newUAC = new UAC(){
            DisplayName = $"uac for {chan.DisplayName}",
            Channels = new List<Channel>() {chan}
        };
        r.RememberUAC(newUAC);
        return RedirectToAction("Details", "Channels", new { Id = Id});
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
