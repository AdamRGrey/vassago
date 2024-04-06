using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;

namespace vassago.Controllers;

public class ChannelsController : Controller
{
    private readonly ILogger<ChannelsController> _logger;
    private readonly ChattingContext _db;

    public ChannelsController(ILogger<ChannelsController> logger, ChattingContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index(string searchString)
    {
        return _db.Channels != null ?
            View(_db.Channels.Include(u => u.ParentChannel).ToList().OrderBy(c => c.LineageSummary)) :
            Problem("Entity set '_db.Channels' is null.");
    }
    public async Task<IActionResult> Details(Guid id)
    {
        return _db.Channels != null ?
            View(await _db.Channels.Include(u => u.ParentChannel).FirstAsync(u => u.Id == id)) :
            Problem("Entity set '_db.Channels' is null.");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}