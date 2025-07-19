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

public class ProtocolController() : Controller
{
    private static Rememberer r = Rememberer.Instance;

    public IActionResult Details(Guid Id)
    {
        var conf = r.SearchProtocolConfig(Id);
        return View(conf.Protocol, conf);
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
        try
        {
            await Reconfigurator.ProtocolInterfaces();
        }
        catch (Exception)
        {
            r.ForgetDiscord(newConf.Id);
            return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        return RedirectToAction("Details", "Protocol", new { Id = newConf.Id });
    }
    [HttpPost]
    public async Task<IActionResult> SubmitDiscord(ProtocolDiscord incoming)
    {
        var confEntity = r.SearchProtocolConfigDiscord(incoming.Id);
        var oldConf = JsonConvert.DeserializeObject<ProtocolDiscord>(JsonConvert.SerializeObject(confEntity));
        confEntity.token = incoming.token;
        Console.WriteLine($"setting setup slash commands to {incoming.SetupSlashCommands}");
        confEntity.SetupSlashCommands = incoming.SetupSlashCommands;
        r.RememberDiscord(confEntity);
        try
        {
            await Reconfigurator.ProtocolInterfaces();
        }
        catch (Exception)
        {
            r.ForgetDiscord(incoming.Id);
            r.RememberDiscord(oldConf);
            await Reconfigurator.ProtocolInterfaces();
            return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        return RedirectToAction("Details", "Protocol", new { Id = confEntity.Id });
    }
    [HttpPost]
    public async Task<IActionResult> DeleteDiscord(Guid id)
    {
        r.ForgetDiscord(id);
        Reconfigurator.ProtocolInterfaces();
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    public async Task<IActionResult> AddTwitch(string username, string oauth)
    {
        var newConf = new ProtocolTwitch()
        {
            username = username,
            oauth = oauth
        };
        r.RememberTwitch(newConf);
        await Reconfigurator.ProtocolInterfaces();
        return RedirectToAction("Details", "Protocol", new { Id = newConf.Id });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitTwitch(ProtocolTwitch incoming)
    {
        var confEntity = r.SearchProtocolConfigTwitch(incoming.Id);
        var oldConf = JsonConvert.DeserializeObject<ProtocolTwitch>(JsonConvert.SerializeObject(confEntity));
        confEntity.username = incoming.username;
        confEntity.oauth = incoming.oauth;
        r.RememberTwitch(confEntity);
        try
        {
            await Reconfigurator.ProtocolInterfaces();
        }
        catch (Exception)
        {
            r.ForgetTwitch(incoming.Id);
            r.RememberTwitch(oldConf);
            await Reconfigurator.ProtocolInterfaces();
            return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        return RedirectToAction("Details", "Protocol", new { Id = confEntity.Id });
    }
    [HttpPost]
    public async Task<IActionResult> DeleteTwitch(Guid id)
    {
        r.ForgetTwitch(id);
        Reconfigurator.ProtocolInterfaces();
        return RedirectToAction("Index", "Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
