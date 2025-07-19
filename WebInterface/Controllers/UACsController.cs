using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.WebInterface.Models;

namespace vassago.WebInterface.Controllers;

public class UACsController() : Controller
{
    Rememberer r = Rememberer.Instance;
    public IActionResult Index()
    {
        return View(r.UACsOverview());
    }
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
