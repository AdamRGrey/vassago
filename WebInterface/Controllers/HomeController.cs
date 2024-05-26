using System.Diagnostics;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using vassago.Models;

namespace vassago.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ChattingContext _db;

    public HomeController(ILogger<HomeController> logger, ChattingContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        var allAccounts = _db.Accounts.ToList();
        var allChannels = _db.Channels.ToList();
        var sb = new StringBuilder();
        sb.Append("[");
        sb.Append("{text: \"channels\", nodes: [");

        var first = true;
        foreach (var topLevelChannel in _db.Channels.Where(x => x.ParentChannel == null))
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
        sb.Append("]},");

        if (allChannels.Any())
        {
            sb.Append("{text: \"orphaned channels\", nodes: [");
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
            sb.Append("]},");
        }
        if (allAccounts.Any())
        {
            sb.Append("{text: \"orphaned accounts\",  nodes: [");
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
        sb.Append("]");
        ViewData.Add("treeString", sb.ToString());
        return View("Index");
    }
    private void serializeChannel(ref StringBuilder sb, ref List<Channel> allChannels, ref List<Account> allAccounts, Channel currentChannel)
    {
        allChannels.Remove(currentChannel);
        sb.Append($"{{\"text\": \"{currentChannel.DisplayName}\"");
        var theseAccounts = allAccounts.Where(a => a.SeenInChannel?.Id == currentChannel.Id);
        allAccounts.RemoveAll(a => a.SeenInChannel?.Id == currentChannel.Id);
        var first = true;
        if (currentChannel.SubChannels != null || theseAccounts != null)
        {
            sb.Append(", \"nodes\": [");
        }
        if (currentChannel.SubChannels != null)
        {
            foreach (var subChannel in currentChannel.SubChannels ?? new List<Channel>())
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
            if (theseAccounts != null)
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
        sb.Append($"{{\"text\": \"{currentUser.DisplayName}\", ");
        var ownedAccounts = allAccounts.Where(a => a.IsUser == currentUser);
        sb.Append("nodes: [");
        sb.Append($"{{\"text\": \"owned accounts:\", \"nodes\": [");
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
