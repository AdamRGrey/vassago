using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;

namespace vassago.Controllers;

public class ChannelsController : Controller
{
    private readonly ILogger<ChannelsController> _logger;
    private readonly ChattingContext _db;

    public ChannelsController(ILogger<ChannelsController> logger, ChattingContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index(string searchString)
    {
        return _db.Channels != null ?
            View(_db.Channels.Include(u => u.ParentChannel).ToList().OrderBy(c => c.LineageSummary)) :
            Problem("Entity set '_db.Channels' is null.");
    }
    public async Task<IActionResult> Details(Guid id)
    {
        if(_db.Channels == null)
            return Problem("Entity set '_db.Channels' is null.");
        //"but adam", says the strawman, "why load *every* channel and walk your way up? surely there's a .Load command that works or something."
        //eh. I checked. Not really. You could make an SQL view that recurses its way up, meh idk how. You could just eagerly load *every* related object...
        //but that would take in all the messages. 
        //realistically I expect this will have less than 1MB of total "channels", and several GB of total messages per (text) channel.
        var AllChannels = await _db.Channels
            .Include(u => u.SubChannels)
            .Include(u => u.Users)
            .Include(u => u.ParentChannel)
            .ToListAsync();
        var channel = AllChannels.First(u => u.Id == id);
        var walker = channel;
        while(walker != null)
        {
            ViewData["breadcrumbs"] = $"<a href=\"{Url.ActionLink(action: "Details", controller: "Channels", values: new {id = walker.Id})}\">{walker.DisplayName}</a>/" + 
                ViewData["breadcrumbs"];
            walker = walker.ParentChannel;
        }
        var sb = new StringBuilder();
        sb.Append("[");
        sb.Append($"{{text: \"{channel.SubChannels?.Count}\", nodes: [");
        var first=true;
        foreach(var subChannel in channel.SubChannels)
        {
            if(!first)
            {
                sb.Append(',');
            }
            else
            {
                first = false;
            }
            sb.Append($"{{\"text\": \"<a href=\\\"{Url.ActionLink(action: "Details", controller: "Channels", values: new {id = subChannel.Id})}\\\">{subChannel.DisplayName}</a>\"}}");
        }
        sb.Append("]}]");

        ViewData.Add("channelsTree", sb.ToString());
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