using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.WebInterface.Models;

namespace vassago.WebInterface.Controllers;

public class AccountsController(ChattingContext db) : Controller
{
    private ChattingContext Database => db;

    public async Task<IActionResult> Index()
    {
        return Database.Accounts != null ?
            View(await Database.Accounts.ToListAsync()) :
            Problem("Entity set '_db.Accounts' is null.");
    }
    public async Task<IActionResult> Details(Guid id)
    {
        var account = await Database.Accounts
            .Include(a => a.IsUser)
            .Include(a => a.SeenInChannel)
            .FirstAsync(a => a.Id == id);
        return Database.Accounts != null ?
            View(account) :
            Problem("Entity set '_db.Accounts' is null.");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}