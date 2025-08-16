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
    public IActionResult SeparateAccount(Guid Id)
    {
        var acc = r.AccountDetail(Id);
        var thisUserId = acc.IsUser.Id;
        r.CarveoutAccount(Id);
        return RedirectToAction("Details", "Users", new { Id = thisUserId });
    }
    [HttpPost]
    public IActionResult UnlinkUAC(Guid UserId, Guid UACid)
    {
        var user = r.UserDetail(UserId);
        var oldUAC = r.UACDetail(UACid);
        oldUAC.Users.Remove(user);
        r.RememberUAC(oldUAC);
        return RedirectToAction("Details", "Users", new { Id = UserId});
    }
    [HttpPost]
    public IActionResult NewUAC(Guid Id)
    {
        Console.WriteLine($"new uac for user {Id}");
        var user = r.UserDetail(Id);
        Console.WriteLine($"user null: {user == null}");
        var newUAC = new UAC(){
            DisplayName = $"uac for {user.DisplayName}",
            Users = new List<User>() {user}
        };
        r.RememberUAC(newUAC);
        return RedirectToAction("Details", "Users", new { Id = Id});
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
