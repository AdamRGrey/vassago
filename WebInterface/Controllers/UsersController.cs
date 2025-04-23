using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.WebInterface.Models;

namespace vassago.WebInterface.Controllers;

public class UsersController(ChattingContext db) : Controller
{
    private ChattingContext Database => db;

    public async Task<IActionResult> Index()
    {
        return Database.Users != null ?
            View(await Database.Users.Include(u => u.Accounts).ToListAsync()) :
            Problem("Entity set '_db.Users' is null.");
    }
    public async Task<IActionResult> Details(Guid id)
    {
        var user = await Database.Users
                .Include(u => u.Accounts)
                .FirstAsync(u => u.Id == id);
        var allTheChannels = await Database.Channels.ToListAsync();
        foreach(var acc in user.Accounts)
        {
            acc.SeenInChannel = allTheChannels.FirstOrDefault(c => c.Id == acc.SeenInChannel.Id);
        }
        return Database.Users != null ?
            View(user) :
            Problem("Entity set '_db.Users' is null.");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
