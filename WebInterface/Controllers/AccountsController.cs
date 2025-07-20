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
    public IActionResult unlinkAccountUser(Guid Id)
    {
        //TODO:unlinkAccountUser
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
    [HttpPost]
    public IActionResult unlinkUAC(Guid Id)
    {
        //TODO:unlinkUAC
        throw new NotImplementedException();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
