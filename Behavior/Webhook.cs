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

    private static List<vassago.Models.Webhook> configuredWebhooks = new List<vassago.Models.Webhook>();
    private ConcurrentDictionary<Guid, WebhookActionOrder> authedCache = new ConcurrentDictionary<Guid, WebhookActionOrder>();
    private HttpClient hc = new HttpClient();

    public static void SetupWebhooks()
    {
        configuredWebhooks = rememberer.Webhooks();
    }

    public override bool ShouldAct(Message message, List<UAC> matchedUACs)
    {
        if (configuredWebhooks?.Count() < 1)
        {
            return false;
        }

        Console.WriteLine($"{configuredWebhooks.Count()} configured webhooks.");
        foreach (var wh in configuredWebhooks)
        {
            var triggerTarget = wh.Trigger;
            Console.WriteLine(triggerTarget);
            foreach (var uacMatch in matchedUACs)
            {
                foreach (var substitution in uacMatch.CommandAlterations)
                {
                    triggerTarget = new Regex(substitution.Key).Replace(triggerTarget, substitution.Value);
                }
            }
            Console.WriteLine($"translated, {triggerTarget}");
            if (Regex.IsMatch(message.TranslatedContent, $"{triggerTarget}\\b", RegexOptions.IgnoreCase))
            {
                var webhookableMessageContent = message.Content.Substring(message.Content.IndexOf(triggerTarget) + triggerTarget.Length + 1);
                Console.WriteLine($"webhookable content: {webhookableMessageContent}");
                if (wh.Uac.Users.Contains(message.Author.IsUser) || wh.Uac.Channels.Contains(message.Channel) || wh.Uac.AccountInChannels.Contains(message.Author))
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
            else
            {
                Console.WriteLine($"{message.TranslatedContent} didn't match {triggerTarget}");
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
        else
        {
            Console.WriteLine("acquired actionorder");
        }
        var msg = translate(actionOrder, message);
        var req = new HttpRequestMessage(new HttpMethod(actionOrder.Conf.Method.ToString()), actionOrder.Conf.Uri);
        var theContentHeader = actionOrder.Conf.Headers?.FirstOrDefault(h => h?.ToLower().StartsWith("content-type:") ?? false);
        Console.WriteLine($"found content header: {theContentHeader}");
        var contentHeaderVal = theContentHeader?.Split(':')?[1]?.ToLower().Trim();
        Console.WriteLine($"contentHeaderrVal: {contentHeaderVal}");
        if (contentHeaderVal != null)
        {
            if(contentHeaderVal =="multipart/form-data")
            {
                req.Content = new System.Net.Http.MultipartFormDataContent(msg);
            }
            try
            {
                req.Content = new System.Net.Http.StringContent(msg);
                req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentHeaderVal);
            }
            catch (Exception e){
                Console.Error.WriteLine("sorry I have to swallow this exception because something else is somewhere and I have no idea where.");
                Console.Error.WriteLine(JsonConvert.SerializeObject(e));
            }
        }
        if(req.Content == null)
        {
            req.Content = new System.Net.Http.StringContent(msg);
        }
        Console.WriteLine($"survived translating string content. request content: {req.Content}");
        if (actionOrder.Conf.Headers?.ToList().Count > 0)
        {
            foreach (var header in actionOrder.Conf.Headers.ToList())
            {
                if (header?.ToLower().StartsWith("content-type:") ?? false)
                {
                    Console.WriteLine("content header; skipping.");
                }
                else
                {
                    Console.WriteLine($"adding header; {header}");
                    req.Headers.Add(header.Split(':')[0], header.Split(':')[0]);
                    Console.WriteLine("survived.");
                }
            }
        }
        else
        {
            Console.WriteLine("no headers to add.");
        }
        Console.WriteLine("about to Send.");
        var response = await hc.SendAsync(req);
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

public class WebhookActionOrder
{
    public vassago.Models.Webhook Conf { get; set; }
    public string webhookContent { get; set; }
}
