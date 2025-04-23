using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.WebInterface.Models;

namespace vassago.WebInterface.Controllers;

public class UACsController() : Controller
{
    public IActionResult Index()
    {
        return View(Rememberer.UACsOverview());
    }
    public IActionResult Details(Guid id)
    {
        return View(Rememberer.SearchUAC(uac => uac.Id == id));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
