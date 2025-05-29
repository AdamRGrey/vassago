using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.ProtocolInterfaces.DiscordInterface;

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

    public class extraSpecialObjectReadGlorifiedTupleFor_PostMessage
    {
        public string messageText;
        public Guid channelId;
    }
    [HttpPost]
    [Route("PostMessage")]
    [Produces("application/json")]
    public IActionResult PostMessage([FromBody]extraSpecialObjectReadGlorifiedTupleFor_PostMessage param)
    {
        return StatusCode(Behaver.Instance.SendMessage(param.channelId, param.messageText).Result);
    }
    public class extraSpecialObjectReadGlorifiedTupleFor_ReplyToMessage
    {
        public string messageText;
        public Guid repliedMessageId;
    }
    [HttpPost]
    [Route("ReplyToMessage")]
    [Produces("application/json")]
    public IActionResult ReplyToMessage([FromBody] extraSpecialObjectReadGlorifiedTupleFor_ReplyToMessage param)
    {
        Console.WriteLine("ReplyToMessage - ${param.repliedMessageId}, {param.messageText}");
        return StatusCode(Behaver.Instance.Reply(param.repliedMessageId, param.messageText).Result);
    }
}
