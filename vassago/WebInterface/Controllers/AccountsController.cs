using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.WebInterface.Models;

namespace vassago.WebInterface.Controllers;

public class AccountsController() : Controller
{
    private Rememberer r = Rememberer.Instance;

    public IActionResult Details(Guid id)
    {
        var acc = r.AccountDetail(id);
        Console.WriteLine(acc);
        Console.WriteLine("is null?");
        Console.WriteLine(acc == null);
        return View(acc);
    }
    [HttpPost]
    public IActionResult UnlinkAccountUser(Guid Id)
    {
        r.CarveoutAccount(Id);
        return RedirectToAction("Details", "Accounts", new { Id = Id });
    }
    [HttpPost]
    public IActionResult UnlinkUAC(Guid AccountId, Guid UACid)
    {
        var acc = r.AccountDetail(AccountId);
        var oldUAC = r.UACDetail(UACid);
        oldUAC.AccountInChannels.Remove(acc);
        r.RememberUAC(oldUAC);
        return RedirectToAction("Details", "Accounts", new { Id = AccountId });
    }
    [HttpPost]
    public IActionResult NewUAC(Guid Id)
    {
        Console.WriteLine($"new uac for account {Id}");
        var acc = r.AccountDetail(Id);
        Console.WriteLine($"account null: {acc == null}");
        var newUAC = new UAC(){
            DisplayName = $"uac for {acc.DisplayName}",
            AccountInChannels = new List<Account>() {acc}
        };
        r.RememberUAC(newUAC);
        return RedirectToAction("Details", "Accounts", new { Id = Id});
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
