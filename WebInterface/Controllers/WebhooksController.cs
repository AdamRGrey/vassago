using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using vassago.Models;
using vassago.WebInterface.Models;

namespace vassago.WebInterface.Controllers;

public class WebhooksController() : Controller
{
    Rememberer r = Rememberer.Instance;

    public async Task<IActionResult> Details(Guid id)
    {
        var webhook = r.Webhook(id);
        // Console.WriteLine(JsonConvert.SerializeObject(webhook));
        return webhook != null ?
            View(webhook) :
            Problem($"webhook {id} is null.");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorPageViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    [HttpPost]
    public IActionResult Submit(Webhook incoming)
    {
        var wh = r.Webhook(incoming.Id) ?? new Webhook();
        wh.Trigger = incoming.Trigger;
        wh.Uri = incoming.Uri;
        wh.Method = incoming.Method;
        wh.Headers = incoming.Headers;
        wh.Content = incoming.Content;
        wh.Description = incoming.Description;
        wh.Uac.DisplayName = "webhook: " + wh.Trigger;
        r.RememberWebhook(wh);
        return RedirectToAction("Details", "Webhooks", new {Id = wh.Id});
    }
    public IActionResult Delete(Guid id)
    {
        r.ForgetWebhook(id);
        return RedirectToAction("Home", "Index");
    }
    public IActionResult Create()
    {
        var wh = new Webhook();
        wh.Uac = new UAC();
        wh.Content = "this webhook needs to be set up";
        wh.Description = "new webhook";
        r.RememberWebhook(wh);

        return RedirectToAction("Details", "Webhooks", new {Id = wh.Id});
    }
    [HttpPost]
    public IActionResult RemoveHeader(Guid Id, int index)
    {
        var wh = r.Webhook(Id);
        wh.Headers.RemoveAt(index);
        r.RememberWebhook(wh);
        return RedirectToAction("Details", "Webhooks", new {Id = wh.Id});
    }
    [HttpPost]
    public IActionResult AddHeader(Guid id)
    {
        var wh = r.Webhook(id);
        wh.Headers ??= [];
        wh.Headers.Add(":");
        r.RememberWebhook(wh);
        return RedirectToAction("Details", "Webhooks", new {Id = wh.Id});
    }
}
