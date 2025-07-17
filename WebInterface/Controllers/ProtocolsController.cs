using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago;
using vassago.Behavior;
using vassago.Models;
using vassago.WebInterface.Models;
using vassago.ProtocolInterfaces;

namespace vassago.WebInterface.Controllers;

public class ProtocolsController() : Controller
{
    private static Rememberer r = Rememberer.Instance;
    public IActionResult Index()
    {
        var conf = r.Configuration() ?? new Configuration();
        ViewData.Add("Serialized", JsonConvert.SerializeObject(conf));
        return View(conf);
    }
    [HttpPost]
    public async Task<IActionResult> AddDiscord(string token)
    {
        var newConf = new ProtocolDiscord()
        {
            token = token,
           SetupSlashCommands = false
        };
        r.RememberDiscord(newConf);
        await Reconfigurator.ProtocolInterfaces();
        return RedirectToAction("Details", "Protocols", new {Id = newConf.Id});
    }
[HttpPost]
    public IActionResult Submit(Configuration incoming)
    {
        // var conf = r.Configuration() ?? new Configuration();
        // conf.ExchangePairsLocation = incoming.ExchangePairsLocation;
        // conf.KafkaBootstrap = incoming.KafkaBootstrap;
        // conf.KafkaName = incoming.KafkaName;
        // conf.reportedApiUrl = incoming.reportedApiUrl;
        // r.RememberConfiguration(conf);
        return RedirectToAction("Index", "Configuration");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

}
