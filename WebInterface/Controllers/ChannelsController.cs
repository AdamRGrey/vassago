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
        else {
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
            new Tuple<Channel, Enumerations.LewdnessFilterLevel, Enumerations.MeannessFilterLevel>(
                channel, channel.EffectivePermissions.LewdnessFilterLevel, channel.EffectivePermissions.MeannessFilterLevel
            ));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
