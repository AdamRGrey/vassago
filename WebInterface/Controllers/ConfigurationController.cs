using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago;
using vassago.Behavior;
using vassago.Models;
using vassago.WebInterface.Models;

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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult AddDiscord(string newToken)
    {
        Console.WriteLine($"remembering discord, {newToken}");
        var conf = r.Configuration();
        conf.DiscordTokens ??=[];
        conf.DiscordTokens.Add(newToken);
        r.RememberConfiguration(conf);
        return RedirectToAction("Index", "Configuration");
    }
}
