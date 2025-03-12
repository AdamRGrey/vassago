using System.Diagnostics;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using vassago.Models;
using vassago.WebInterface.Models;

namespace vassago.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var allAccounts = Rememberer.AccountsOverview();
        var allChannels = Rememberer.ChannelsOverview();
        Console.WriteLine($"accounts: {allAccounts?.Count ?? 0}, channels: {allChannels?.Count ?? 0}");
        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append("{text: \"channels\", expanded:true, nodes: [");

        var first = true;
        var topLevelChannels = Rememberer.ChannelsOverview().Where(x => x.ParentChannel == null);
        foreach (var topLevelChannel in topLevelChannels)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                sb.Append(',');
            }

            serializeChannel(ref sb, ref allChannels, ref allAccounts, topLevelChannel);
        }
        sb.Append("]}");

        if (allChannels.Any())
        {
            sb.Append(",{text: \"orphaned channels\", expanded:true, nodes: [");
            first = true;
            while (true)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(',');
                }
                serializeChannel(ref sb, ref allChannels, ref allAccounts, allChannels.First());
                if (!allChannels.Any())
                {
                    break;
                }
            }
            sb.Append("]}");
        }
        if (allAccounts.Any())
        {
            sb.Append(",{text: \"channelless accounts\", expanded:true, nodes: [");
            first = true;
            foreach (var acc in allAccounts)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(',');
                }
                serializeAccount(ref sb, acc);
            }
            sb.Append("]}");
        }
        var users = Rememberer.UsersOverview();// _db.Users.ToList();
        if(users.Any())
        {
            sb.Append(",{text: \"users\", expanded:true, nodes: [");
            first=true;
            //refresh list; we'll be knocking them out again in serializeUser
            allAccounts = Rememberer.AccountsOverview();
            foreach(var user in users)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(',');
                }
                serializeUser(ref sb, ref allAccounts, user);
            }
            sb.Append("]}");
        }
        sb.Append(']');
        ViewData.Add("treeString", sb.ToString());
        return View("Index");
    }
    private void serializeChannel(ref StringBuilder sb, ref List<Channel> allChannels, ref List<Account> allAccounts, Channel currentChannel)
    {
        allChannels.Remove(currentChannel);
        //"but adam", you say, "there's an href attribute, why make a link?" because that makes the entire bar a link, and trying to expand the node will probably click the link
        sb.Append($"{{\"text\": \"<a href=\\\"{Url.ActionLink(action: "Details", controller: "Channels", values: new {id = currentChannel.Id})}\\\">{currentChannel.DisplayName}</a>\"");
        sb.Append(", expanded:true ");
        var theseAccounts = allAccounts.Where(a => a.SeenInChannel?.Id == currentChannel.Id).ToList();
        allAccounts.RemoveAll(a => a.SeenInChannel?.Id == currentChannel.Id);
        var first = true;
        if (currentChannel.SubChannels != null || theseAccounts != null)
        {
            sb.Append(", \"nodes\": [");
        }
        if (currentChannel.SubChannels != null)
        {
            foreach (var subChannel in currentChannel.SubChannels)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(',');
                }
                serializeChannel(ref sb, ref allChannels, ref allAccounts, subChannel);
            }
            if (theseAccounts != null && !first) //"first" here tells us that we have at least one subchannel
            {
                sb.Append(',');
            }
        }
        if (theseAccounts != null)
        {
            first = true;
            sb.Append($"{{\"text\": \"(accounts: {theseAccounts.Count()})\", \"expanded\":true, nodes:[");
            foreach (var account in theseAccounts)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(',');
                }
                serializeAccount(ref sb, account);
            }
            sb.Append("]}");
        }
        sb.Append("]}");
    }
    private void serializeAccount(ref StringBuilder sb, Account currentAccount)
    {
        sb.Append($"{{\"text\": \"{currentAccount.DisplayName}\"}}");
    }
    private void serializeUser(ref StringBuilder sb, ref List<Account> allAccounts, User currentUser)
    {
        sb.Append($"{{\"text\": \"<a href=\\\"{Url.ActionLink(action: "Details", controller: "Users", values: new {id = currentUser.Id})}\\\">");
        sb.Append(currentUser.DisplayName);
        sb.Append("</a>\", ");
        var ownedAccounts = allAccounts.Where(a => a.IsUser == currentUser);
        sb.Append("nodes: [");
        sb.Append($"{{\"text\": \"owned accounts:\", \"expanded\":true, \"nodes\": [");
        if (ownedAccounts != null)
        {
            foreach (var acc in ownedAccounts)
            {
                serializeAccount(ref sb, acc);
                sb.Append(',');
            }
        }
        sb.Append("]}]}");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
