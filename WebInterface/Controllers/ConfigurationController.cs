using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago;
using vassago.Behavior;
using vassago.Models;
using vassago.WebInterface.Models;
using vassago.TwitchInterface;

namespace vassago.WebInterface.Controllers;

public class ConfigurationController() : Controller
{
    private static Rememberer r = Rememberer.Instance;
    public IActionResult Index()
    {
        var conf = r.Configuration() ?? new Configuration();
        ViewData.Add("Serialized", JsonConvert.SerializeObject(conf));
        return View(conf);
    }
    [HttpPost]
    public IActionResult Submit(Configuration incoming)
    {
        var conf = r.Configuration() ?? new Configuration();
        conf.DiscordTokens = incoming.DiscordTokens;
        conf.TwitchConfigs = incoming.TwitchConfigs;
        conf.ExchangePairsLocation = incoming.ExchangePairsLocation;
        conf.SetupDiscordSlashCommands = incoming.SetupDiscordSlashCommands;
        conf.Webhooks = incoming.Webhooks;
        conf.KafkaBootstrap = incoming.KafkaBootstrap;
        conf.KafkaName = incoming.KafkaName;
        conf.reportedApiUrl = incoming.reportedApiUrl;
        r.RememberConfiguration(conf);
        return RedirectToAction("Index", "Configuration");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult AddDiscord(string newToken)
    {
        Console.WriteLine($"adding discord, {newToken}");
        var conf = r.Configuration();
        conf.DiscordTokens ??= [];
        conf.DiscordTokens.Add(newToken);
        r.RememberConfiguration(conf);
        return RedirectToAction("Index", "Configuration");
    }

    [HttpPost]
    public IActionResult RemoveDiscord(int index)
    {
        Console.WriteLine($"removing discord[{index}]");
        var conf = r.Configuration();
        if (conf.DiscordTokens?.Count <= index)
        {
            Console.Error.WriteLine("error removing discord {index} from configuration, only have {conf.DiscordTokens?.Count}.");
            return RedirectToAction("Index", "Configuration");
        }

        conf.DiscordTokens.RemoveAt(index);
        r.RememberConfiguration(conf);
        return RedirectToAction("Index", "Configuration");
    }

    [HttpPost]
    public IActionResult AddTwitch(string newUsername, string newOauth)
    {
        Console.WriteLine($"adding twitch, {newUsername}/{newOauth}");
        var conf = r.Configuration();
        conf.TwitchConfigs ??= [];
        var thisOne = new TwitchConfig()
        {
            username = newUsername,
            oauth = newOauth
        };
        conf.TwitchConfigs.Add(JsonConvert.SerializeObject(thisOne));
        r.RememberConfiguration(conf);
        return RedirectToAction("Index", "Configuration");
    }

    [HttpPost]
    public IActionResult RemoveTwitch(int index)
    {
        Console.WriteLine($"removing twitch[{index}]");
        var conf = r.Configuration();
        if (conf.TwitchConfigs?.Count <= index)
        {
            Console.Error.WriteLine("error removing twitch {index} from configuration, only have {conf.TwitchConfigs?.Count}.");
            return RedirectToAction("Index", "Configuration");
        }
        conf.TwitchConfigs.RemoveAt(index);
        r.RememberConfiguration(conf);
        return RedirectToAction("Index", "Configuration");
    }

    [HttpPost]
    public IActionResult AddWebhook(WebhookConf newWebhook)
    {
        Console.WriteLine($"adding webhook, {newWebhook}");
        var conf = r.Configuration();
        conf.Webhooks??= [];
        conf.Webhooks.Add(JsonConvert.SerializeObject(newWebhook));
        r.RememberConfiguration(conf);
        return RedirectToAction("Index", "Configuration");
    }

    [HttpPost]
    public IActionResult RemoveWebhook(int index)
    {
        Console.WriteLine($"removing webhook[{index}]");
        var conf = r.Configuration();
        if (conf.Webhooks?.Count <= index)
        {
            Console.Error.WriteLine("error removing webhook {index} from configuration, only have {conf.Webhooks?.Count}.");
            return RedirectToAction("Index", "Configuration");
        }

        conf.Webhooks.RemoveAt(index);
        r.RememberConfiguration(conf);
        return RedirectToAction("Index", "Configuration");
    }
}
