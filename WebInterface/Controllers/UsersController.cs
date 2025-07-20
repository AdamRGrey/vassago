using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.WebInterface.Models;

namespace vassago.WebInterface.Controllers;

public class UsersController() : Controller
{
    Rememberer r = Rememberer.Instance;

    public async Task<IActionResult> Details(Guid id)
    {
        var user = r.UserDetail(id);
        if (user.Accounts != null) foreach (var acc in user.Accounts)
            {
                acc.SeenInChannel = r.SearchChannel(c => c.Id == acc.SeenInChannel.Id);
            }
        return View(user);
    }
    [HttpPost]
    public IActionResult SeparateAccount(Guid UserId, Guid AccountId)
    {
        //TODO: separate account, webinterface
        throw new NotImplementedException();
        return View();
    }
    [HttpPost]
    public IActionResult UnlinkUAC(Guid UserId, Guid UACid)
    {
        //TODO: unlink UAC from User webinterface
        throw new NotImplementedException();
        return View();
    }
    [HttpPost]
    public IActionResult newUAC(Guid Id)
    {
        //TODO:newUAC
        throw new NotImplementedException();
        return View();
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
