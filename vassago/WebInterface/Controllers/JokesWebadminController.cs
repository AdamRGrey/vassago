using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using vassago.Models;
using vassago.WebInterface.Models;

namespace vassago.WebInterface.Controllers;

public class JokesWebadminController() : Controller
{
    private static Rememberer r = Rememberer.Instance;
    public IActionResult Index()
    {
        return View(r.JokesOverview());
    }


    [HttpPost]
    public IActionResult Submit(Joke joke)
    {
        Console.WriteLine($"[admin ui][jokes][update]");
        if (string.IsNullOrWhiteSpace(joke.PrimaryText))
        {
            Console.Error.WriteLine($"[admin ui][jokes][update] - invalid joke submitted; {JsonConvert.SerializeObject(joke)}");
            return RedirectToAction("Index", "JokesWebadmin");
        }
        var fromDb = r.SearchJoke(joke.Id);
        fromDb.PrimaryText = joke.PrimaryText;
        fromDb.SecondaryText = joke.SecondaryText;
        fromDb.LewdnessConformity = joke.LewdnessConformity;
        fromDb.MeannessConformity = joke.MeannessConformity;
        r.RememberJoke(fromDb);
        return RedirectToAction("Index", "JokesWebadmin");
    }
    [HttpPost]
    public IActionResult Delete(Guid id)
    {
        r.ForgetJoke(id);
        return RedirectToAction("Index", "JokesWebadmin");
    }
    [HttpPost]
    public IActionResult New(Joke joke)
    {
        Console.WriteLine($"[admin ui][jokes][new]");
        r.RememberJoke(joke);
        return RedirectToAction("Index", "JokesWebadmin");
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
