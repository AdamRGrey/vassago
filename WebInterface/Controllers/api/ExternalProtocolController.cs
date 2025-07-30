using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.ProtocolInterfaces;

namespace vassago.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class ExternalProtocolController : ControllerBase
{
    /* "you", the external protocol, talk to "me", vassago, here.
     * you tell me when chat events happen - message received, etc.
     * I will tell you when I need you to do things - (to respond to this command) send this text in this channel, etc.
     * if you are "operating in restful style", that means you ask me here for the list of commands I have for you.
     * I will handle bookkeeping, *except*: what I call "external IDs" are what you handle for you.
     * you need to keep track of an "external ID" that corresponds to *you* - this should not be a running instance of you, but rather if you have your own database, that database.
     * if you support separate channels, i will tell you my ID (which I may forget about, change, etc. shit happens.) and I will give them their own "external ID", which you will need to handle.
     * regardless of if you support channels, there will be a "protocol as channel" for you.
     * if you support channels, all your channels must be in a hierarchy below that.
     * "I" (vassago) am the single source of truth. so if I'm convinced an external ID is yours, trust me bro.
     * //TOOD: if you absolutely cannot hanlde it... we trash it.
     */
    private readonly ILogger<ExternalProtocolController> _logger;

    public ExternalProtocolController(ILogger<ExternalProtocolController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("GetCommands")]
    [ProducesResponseType<List<ExternalCommand>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetCommands(string protocolExternalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if(extproto == null)
            return NotFound();

        var toReturn = extproto.CommandQueue;
        extproto.CommandQueue = new List<ExternalCommand>();
        return Ok(toReturn);
    }

    ///<summary>and/or reconnect</summary>
    [HttpPost]
    [Route("Connect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Connect(ProtocolExternal conf)
    {
        if(String.IsNullOrWhiteSpace(conf?.ExternalId))
        {
            ModelState.AddModelError(nameof(conf.ExternalId), "ExternalId is required.");
            return BadRequest(ModelState);
        }
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == conf.ExternalId)
            as ExternalRestful;

        if(extproto == null)
        {
            //extproto = new
         throw new NotImplementedException();
        }
        else
        {
         throw new NotImplementedException();
        }

         throw new NotImplementedException();
        // return StatusCode(Behaver.Instance.SendMessage(channelId, messageText).Result);
    }

    ///<summary>you intend to reconnect later</summary>
    [HttpPost]
    [Route("Disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Disconnect(string protocolExternalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if(extproto == null)
            return NotFound();

         throw new NotImplementedException();
        // return StatusCode(Behaver.Instance.SendMessage(channelId, messageText).Result);
    }

    ///<summary>you do not intend to reconnect</summary>
    [HttpPost]
    [Route("Disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Die(string protocolExternalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if(extproto == null)
            return NotFound();

         throw new NotImplementedException();
        // return StatusCode(Behaver.Instance.SendMessage(channelId, messageText).Result);
    }
    [HttpPost]
    [Route("MessageReceived")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult MessageReceived(string protocolExternalId, Message message)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if(extproto == null)
            return NotFound();

         throw new NotImplementedException();
        // return StatusCode(Behaver.Instance.SendMessage(channelId, messageText).Result);
    }

    ///<summary>"created" as far as our bookkeeping is concerned. "first spotted" would be valid. etc.</summary>
    [HttpPost]
    [Route("AccountCreated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AccountCreated(string protocolExternalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if(extproto == null)
            return NotFound();

         throw new NotImplementedException();
        // return StatusCode(Behaver.Instance.SendMessage(channelId, messageText).Result);
    }
    ///<summary>"created" as far as our bookkeeping is concerned. "first spotted" would be valid. etc.</summary>
    [HttpPost]
    [Route("ChannelCreated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ChannelCreated(string protocolExternalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if(extproto == null)
            return NotFound();

         throw new NotImplementedException();
        // return StatusCode(Behaver.Instance.SendMessage(channelId, messageText).Result);
    }
}
