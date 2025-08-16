using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.WebInterface.Models;

namespace vassago.WebInterface.Controllers;

public class UACsController() : Controller
{
    Rememberer r = Rememberer.Instance;

    public IActionResult Details(Guid id)
    {
        return View(r.SearchUAC(uac => uac.Id == id));
    }

    [HttpPost]
    public IActionResult Create()
    {
        var newUAC = new UAC()
        {
            DisplayName = "[arbitrary uac]"
        };
        r.RememberUAC(newUAC);

        return RedirectToAction("Details", "UACs", new { Id = newUAC.Id });
    }
    [HttpPost]
    public IActionResult Submit(UAC incomingUac)
    {
        Console.WriteLine(JsonConvert.SerializeObject(incomingUac));
        var fromDb = r.SearchUAC(uac => uac.Id == incomingUac.Id);
        if(!String.IsNullOrWhiteSpace(incomingUac.DisplayName))
            fromDb.DisplayName = incomingUac.DisplayName;
        fromDb.Description = incomingUac.Description;
        fromDb.Translations = incomingUac.Translations;
        fromDb.CommandAlterations = incomingUac.CommandAlterations;
        r.RememberUAC(fromDb);
        return RedirectToAction("Details", "UACs", new { Id = fromDb.Id });
    }
    [HttpPost]
    public IActionResult Delete(Guid Id)
    {
        var fromDb = r.SearchUAC(uac => uac.Id == Id);
        r.ForgetUAC(fromDb);
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    public IActionResult LinkUser(Guid UACId, Guid UserId)
    {
        var uacFromDb = r.SearchUAC(uac => uac.Id == UACId);
        var userFromDb = r.SearchUser(u => u.Id == UserId);
        uacFromDb.Users ??= [];
        uacFromDb.Users.Add(userFromDb);
        return RedirectToAction("Details", "UACs", new { Id = UACId });
    }
    [HttpPost]
    public IActionResult LinkAccount(Guid UACId, Guid AccountId)
    {
        var uacFromDb = r.SearchUAC(uac => uac.Id == UACId);
        var AccountFromDb = r.SearchAccount(u => u.Id == AccountId);
        uacFromDb.AccountInChannels ??= [];
        uacFromDb.AccountInChannels.Add(AccountFromDb);
        return RedirectToAction("Details", "UACs", new { Id = UACId });
    }
    [HttpPost]
    public IActionResult LinkChannel(Guid UACId, Guid ChannelId)
    {
        var uacFromDb = r.SearchUAC(uac => uac.Id == UACId);
        var ChannelFromDb = r.SearchChannel(u => u.Id == ChannelId);
        uacFromDb.Channels ??= [];
        uacFromDb.Channels.Add(ChannelFromDb);
        return RedirectToAction("Details", "UACs", new { Id = UACId });
    }
    [HttpPost]
    public IActionResult UnlinkUser(Guid UACId, Guid UserId)
    {
        var uacFromDb = r.SearchUAC(uac => uac.Id == UACId);
        var UserFromDb = r.SearchUser(u => u.Id == UserId);
        uacFromDb.Users.Remove(UserFromDb);
        r.RememberUAC(uacFromDb);
        return RedirectToAction("Details", "UACs", new { Id = UACId });
    }
    [HttpPost]
    public IActionResult UnlinkAccount(Guid UACId, Guid AccountId)
    {
        var uacFromDb = r.SearchUAC(uac => uac.Id == UACId);
        var AccountFromDb = r.SearchAccount(u => u.Id == AccountId);
        uacFromDb.AccountInChannels.Remove(AccountFromDb);
        r.RememberUAC(uacFromDb);
        return RedirectToAction("Details", "UACs", new { Id = UACId });
    }
    [HttpPost]
    public IActionResult UnlinkChannel(Guid UACId, Guid ChannelId)
    {
        var uacFromDb = r.SearchUAC(uac => uac.Id == UACId);
        var ChannelFromDb = r.SearchChannel(u => u.Id == ChannelId);
        uacFromDb.Channels.Remove(ChannelFromDb);
        r.RememberUAC(uacFromDb);
        return RedirectToAction("Details", "UACs", new { Id = UACId });
    }
    [HttpPost]
    public IActionResult AddTranslation(Guid Id)
    {
        var uacFromDb = r.SearchUAC(uac => uac.Id == Id);
        uacFromDb.Translations ??=[];
        uacFromDb.Translations[Guid.NewGuid().ToString()] = Guid.NewGuid().ToString();
        r.RememberUAC(uacFromDb);
        return RedirectToAction("Details", "UACs", new { Id = Id });
    }
    [HttpPost]
    public IActionResult AddCommandAlteration(Guid Id)
    {
        var uacFromDb = r.SearchUAC(uac => uac.Id == Id);
        uacFromDb.CommandAlterations ??=[];
        uacFromDb.CommandAlterations[Guid.NewGuid().ToString()] = Guid.NewGuid().ToString();
        r.RememberUAC(uacFromDb);
        return RedirectToAction("Details", "UACs", new { Id = Id });
    }
    [HttpPost]
    public IActionResult RemoveTranslation(Guid Id, string key)
    {
        var uacFromDb = r.SearchUAC(uac => uac.Id == Id);
        uacFromDb.Translations.Remove(key);
        r.RememberUAC(uacFromDb);
        return RedirectToAction("Details", "UACs", new { Id = Id });
    }
    [HttpPost]
    public IActionResult RemoveCommandAlteration(Guid Id,  string key)
    {
        var uacFromDb = r.SearchUAC(uac => uac.Id == Id);
        uacFromDb.CommandAlterations.Remove(key);
        r.RememberUAC(uacFromDb);
        return RedirectToAction("Details", "UACs", new { Id = Id });
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
