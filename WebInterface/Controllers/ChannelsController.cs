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
    public IActionResult Details(Guid id)
    {
        var allChannels = Rememberer.ChannelsOverview();
        if (allChannels == null)
            return Problem("no channels.");

        var channel = allChannels.FirstOrDefault(u => u.Id == id);
        if (channel == null)
        {
            return Problem($"couldn't find channle {id}");
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
