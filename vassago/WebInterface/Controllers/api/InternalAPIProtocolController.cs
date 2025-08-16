using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.ProtocolInterfaces;

namespace vassago.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class InternalAPIProtocolController : ControllerBase
{
    private readonly ILogger<InternalAPIProtocolController> _logger;

    public InternalAPIProtocolController(ILogger<InternalAPIProtocolController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("PostMessage")]
    [Produces("application/json")]
    public IActionResult PostMessage(string messageText, Guid channelId)
    {
        return StatusCode(Behaver.Instance.SendMessage(channelId, messageText).Result);
    }

    [HttpPost]
    [Route("ReplyToMessage")]
    [Produces("application/json")]
    public IActionResult ReplyToMessage(string messageText, Guid repliedMessageId)
    {
        Console.WriteLine($"ReplyToMessage - {repliedMessageId}, {messageText}");
        return StatusCode(Behaver.Instance.Reply(repliedMessageId, messageText).Result);
    }
    [HttpPost]
    [Route("SendFile")]
    [Produces("application/json")]
    public IActionResult SendFile(Guid channelId, string accompanyingText, string base64dData, string filename)
    {
        Console.WriteLine($"SendFile- {channelId}, {filename} (base64'd, {base64dData?.Length} chars), {accompanyingText}");
        return StatusCode(Behaver.Instance.SendFile(channelId, base64dData, filename, accompanyingText).Result);
    }

    [HttpPost]
    [Route("ReactToMessage")]
    [Produces("application/json")]
    public IActionResult ReactToMessage(string reactionString, Guid reactedMessageId)
    {
        Console.WriteLine($"ReactToMessage- {reactedMessageId}, {reactionString}");
        return StatusCode(Behaver.Instance.React(reactedMessageId, reactionString).Result);
    }
}
