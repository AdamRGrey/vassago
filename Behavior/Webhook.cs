namespace vassago.Behavior;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vassago.Models;
using Newtonsoft.Json;

[StaticPlz]
public class Webhook : Behavior
{
    public override string Name => "Webhook";

    public override string Trigger => "!hook";

    public override string Description => "call a webhook";

    private static List<WebhookConf> configuredWebhooks = new List<WebhookConf>();
    private ConcurrentDictionary<Guid, WebhookActionOrder> authedCache = new ConcurrentDictionary<Guid, WebhookActionOrder>();
    private HttpClient hc = new HttpClient();

    public static void SetupWebhooks(IEnumerable<string> confSection)
    {
        //configuredWebhooks = confSection.Get<List<vassago.Behavior.WebhookConf>>();
        if(confSection != null) foreach (var confLine in confSection)
        {
            var conf = JsonConvert.DeserializeObject<WebhookConf>(confLine);
            var confName = $"Webhook: {conf.Trigger}";
            var changed = false;
            var myUAC = rememberer.SearchUAC(uac => uac.OwnerId == conf.uacID);
            if (myUAC == null)
            {
                myUAC = new()
                {
                    OwnerId = conf.uacID,
                    DisplayName = confName,
                    Description = conf.Description
                };
                changed = true;
                rememberer.RememberUAC(myUAC);
            }
            else
            {
                if (myUAC.DisplayName != confName)
                {
                    myUAC.DisplayName = confName;
                    changed = true;
                }
                if (myUAC.Description != conf.Description)
                {
                    myUAC.Description = conf.Description;
                    changed = true;
                }
            }
            if (changed)
                rememberer.RememberUAC(myUAC);
        }
    }

    public override bool ShouldAct(Message message, List<UAC> matchedUACs)
    {
        if (configuredWebhooks?.Count() < 1)
        {
            return false;
        }

        foreach (var wh in configuredWebhooks)
        {
            var triggerTarget = wh.Trigger;
            foreach (var uacMatch in matchedUACs)
            {
                foreach (var substitution in uacMatch.CommandAlterations)
                {
                    triggerTarget = new Regex(substitution.Key).Replace(triggerTarget, substitution.Value);
                }
            }
            if (Regex.IsMatch(message.TranslatedContent, $"\\b{triggerTarget}\\b", RegexOptions.IgnoreCase))
            {
                var webhookableMessageContent = message.Content.Substring(message.Content.IndexOf(triggerTarget) + triggerTarget.Length + 1);
                Console.WriteLine($"webhookable content: {webhookableMessageContent}");
                var uacConf = rememberer.SearchUAC(uac => uac.OwnerId == wh.uacID);
                if (uacConf.Users.Contains(message.Author.IsUser) || uacConf.Channels.Contains(message.Channel) || uacConf.AccountInChannels.Contains(message.Author))
                {
                    Console.WriteLine("webhook UAC passed, preparing WebhookActionOrder");
                    authedCache.TryAdd(message.Id, new WebhookActionOrder()
                    {
                        Conf = wh,
                        webhookContent = webhookableMessageContent,
                    });
                    Console.WriteLine($"added {message.Id} to authedcache");
                    return true;
                }
            }
        }
        return false;
    }
    public override async Task<bool> ActOn(Message message)
    {
        Console.WriteLine($"hi i'm ActOn. acting on {message.Id}");
        WebhookActionOrder actionOrder;
        if (!authedCache.TryRemove(message.Id, out actionOrder))
        {
            Console.Error.WriteLine($"{message.Id} was supposed to act, but authedCache doesn't have it! it has {authedCache?.Count()} other stuff, though.");
            return false;
        }
        var msg = translate(actionOrder, message);
        var req = new HttpRequestMessage(new HttpMethod(actionOrder.Conf.Method.ToString()), actionOrder.Conf.Uri);
        var theContentHeader = actionOrder.Conf.Headers?.FirstOrDefault(h => h[0]?.ToLower() == "content-type");
        if (theContentHeader != null)
        {
            switch (theContentHeader[1]?.ToLower())
            {
                //json content is constructed some other weird way.
                case "multipart/form-data":
                    req.Content = new System.Net.Http.MultipartFormDataContent(msg);
                    break;
                default:
                    req.Content = new System.Net.Http.StringContent(msg);
                    break;
            }
            req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(theContentHeader[1]?.ToLower());
        }
        if (req.Content == null)
        {
            req.Content = new System.Net.Http.StringContent(msg);
        }
        Console.WriteLine($"survived translating string content. request content: {req.Content}");
        if (actionOrder.Conf.Headers?.ToList().Count > 0)
        {
            Console.WriteLine("will add headers.");
            foreach (var kvp in actionOrder.Conf.Headers.ToList())
            {
                if (kvp[0] == theContentHeader[0])
                {
                    Console.WriteLine("content header; skipping.");
                }
                else
                {
                    Console.WriteLine($"adding header; {kvp[0]}: {kvp[1]}");
                    req.Headers.Add(kvp[0], kvp[1]);
                    Console.WriteLine("survived.");
                }
            }
        }
        else
        {
            Console.WriteLine("no headers to add.");
        }
        Console.WriteLine("about to Send.");
        var response = hc.Send(req);
        Console.WriteLine($"{response.StatusCode} - {response.ReasonPhrase}");
        if (!response.IsSuccessStatusCode)
        {
            var tragedy = $"{response.StatusCode} - {response.ReasonPhrase} - {await response.Content.ReadAsStringAsync()}";
            Console.Error.WriteLine(tragedy);
            Behaver.Instance.Reply(message.Id, tragedy);
        }
        else
        {
            Behaver.Instance.Reply(message.Id, await response.Content.ReadAsStringAsync());
        }
        return true;
    }
    private string translate(WebhookActionOrder actionOrder, Message message)
    {
        if (string.IsNullOrWhiteSpace(actionOrder.Conf.Content))
            return "";
        var msgContent = actionOrder.Conf.Content.Replace("{text}", actionOrder.webhookContent);
        msgContent = msgContent.Replace("{msgid}", message.Id.ToString());
        msgContent = msgContent.Replace("{account}", message.Author.DisplayName.ToString());
        msgContent = msgContent.Replace("{user}", message.Author.IsUser.DisplayName.ToString());
        return msgContent;
    }
}

public class WebhookConf
{
    public Guid uacID { get; set; }
    public string Trigger { get; set; }
    public Uri Uri { get; set; }
    //public HttpMethod Method { get; set; }
    public Enumerations.HttpVerb Method { get; set; }
    public List<List<string>> Headers { get; set; }
    public string Content { get; set; }
    public string Description { get; set; }
}
public class WebhookActionOrder
{
    public WebhookConf Conf { get; set; }
    public string webhookContent { get; set; }
}
