using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;

namespace vassago.Controllers;

public class UsersController : Controller
{
    private readonly ILogger<UsersController> _logger;
    private readonly ChattingContext _db;

    public UsersController(ILogger<UsersController> logger, ChattingContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index(string searchString)
    {
        return _db.Users != null ?
            View(await _db.Users.Include(u => u.Accounts).ToListAsync()) :
            Problem("Entity set '_db.Users' is null.");
    }
    public async Task<IActionResult> Details(Guid id)
    {
        var user = await _db.Users
                .Include(u => u.Accounts)
                .FirstAsync(u => u.Id == id);
        var allTheChannels = await _db.Channels.ToListAsync();
        foreach(var acc in user.Accounts)
        {
            acc.SeenInChannel = allTheChannels.FirstOrDefault(c => c.Id == acc.SeenInChannel.Id);
        }
        return _db.Users != null ?
            View(user) :
            Problem("Entity set '_db.Users' is null.");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}