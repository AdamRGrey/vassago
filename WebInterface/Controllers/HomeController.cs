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
    private static Rememberer r = Rememberer.Instance;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(new OverviewViewModel
        {
            Protocols = r.ProtocolsOverview(),
            Accounts = r.AccountsOverview(),
            Channels = r.ChannelsOverview(),
            Webhooks = r.Webhooks(),
            UACs = r.UACsOverview(),
            Users = r.UsersOverview(),
        });
    }
    private void serializeChannel(ref StringBuilder sb, ref List<Channel> allChannels, ref List<Account> allAccounts, Channel currentChannel)
    {
        allChannels.Remove(currentChannel);
        //"but adam", you say, "there's an href attribute, why make a link?" because that makes the entire bar a link, and trying to expand the node will probably click the link
        sb.Append($"{{\"text\": \"<a href=\\\"{Url.ActionLink(action: "Details", controller: "Channels", values: new { id = currentChannel.Id })}\\\">{currentChannel.DisplayName}</a>\"");
        sb.Append(", expanded:true ");
        var theseAccounts = allAccounts.Where(a => a.SeenInChannel?.Id == currentChannel.Id).ToList();
        allAccounts.RemoveAll(a => a.SeenInChannel?.Id == currentChannel.Id);
        var first = true;
        if (currentChannel.SubChannels != null || theseAccounts != null)
        {
            sb.Append(", \"nodes\": [");
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
            sb.Append(']');
        }
        sb.Append('}');
    }
    private void serializeAccount(ref StringBuilder sb, Account currentAccount)
    {
        sb.Append($"{{\"text\": \"<a href=\\\"{Url.ActionLink(action: "Details", controller: "Accounts", values: new { id = currentAccount.Id })}\\\">{currentAccount.DisplayName}</a>\"}}");
    }
    private void serializeUser(ref StringBuilder sb, ref List<Account> allAccounts, User currentUser)
    {
        sb.Append($"{{\"text\": \"<a href=\\\"{Url.ActionLink(action: "Details", controller: "Users", values: new { id = currentUser.Id })}\\\">");
        sb.Append(currentUser.DisplayName);
        sb.Append("</a>\", ");
        var ownedAccounts = allAccounts.Where(a => a.IsUser == currentUser);
        if (ownedAccounts?.Count() > 0)
        {
            sb.Append("nodes: [");
            sb.Append($"{{\"text\": \"owned accounts:\", \"expanded\":true, \"nodes\": [");
            var first = true;
            foreach (var acc in ownedAccounts)
            {
                if (!first)
                    sb.Append(',');
                serializeAccount(ref sb, acc);
                first = false;
            }
            sb.Append("]}]");
        }
        sb.Append("}");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
