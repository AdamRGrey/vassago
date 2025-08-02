namespace vassago.Controllers.api;

using Microsoft.AspNetCore.Mvc;
using vassago.Models;
using vassago.ProtocolInterfaces;
using Newtonsoft.Json;

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
    private Rememberer r = Rememberer.Instance;

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
        if (extproto == null)
            return NotFound();

        var toReturn = extproto.CommandQueue;
        extproto.CommandQueue = new List<ExternalCommand>();
        return Ok(toReturn);
    }

    ///<summary>for the first time. reconnecting, you're on your own... until I do the Webhook and other style.</summary>
    [HttpPost]
    [Route("Connect")]
    [ProducesResponseType<ProtocolExternal>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Connect(ProtocolExternal incoming)
    {
        if (String.IsNullOrWhiteSpace(incoming?.ExternalId))
        {
            ModelState.AddModelError(nameof(incoming.ExternalId), "ExternalId is required.");
            return BadRequest(ModelState);
        }
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == incoming.ExternalId)
            as ExternalRestful;
        var confEntity = r.SearchProtocolConfigExternal(incoming.ExternalId);

        if (extproto == null)
        {
            confEntity.Style = incoming.Style;
            r.RememberExternal(confEntity);
            try
            {
                await Reconfigurator.ProtocolInterfaces();
                return Ok(extproto);
            }
            catch (Exception e)
            {
                r.ForgetExternal(incoming.Id);
                await Reconfigurator.ProtocolInterfaces();
                return BadRequest(e);
            }
        }
        else
        {
            var oldConf = JsonConvert.DeserializeObject<ProtocolExternal>(JsonConvert.SerializeObject(confEntity));
            if (confEntity.Style != incoming.Style)
            {
                confEntity.Style = incoming.Style;
                r.RememberExternal(confEntity);
                try
                {
                    await Reconfigurator.ProtocolInterfaces();
                    return Ok(extproto);
                }
                catch (Exception e)
                {
                    r.ForgetExternal(incoming.Id);
                    r.RememberExternal(oldConf);
                    await Reconfigurator.ProtocolInterfaces();
                    return BadRequest(e);
                }
            }
            //no changes. "status code two hundred ellipsis question mark: ok...?"
            return Ok(extproto);
        }
    }

    ///<summary>if you do not intend to reconnect. when (read: if ever) i do webhook style, i'll want a temporary disconnect notification</summary>
    [HttpPost]
    [Route("Disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Die(string protocolExternalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        r.ForgetExternal(extproto.ConfigurationEntity.Id);
        Reconfigurator.ProtocolInterfaces();

        return Ok();
    }
    [HttpPost]
    [Route("MessageReceived")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MessageReceived((string protocolExternalId, Message message, string authorExternalId, string channelExternalId) parameters)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == parameters.protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        return StatusCode(await extproto.ExternalMessageReceive(parameters.message, parameters.authorExternalId, parameters.channelExternalId));
    }
    [HttpPost]
    [Route("MessageUpdated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MessageUpdated((string protocolExternalId, Message message, string authorExternalId, string channelExternalId) parameters)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == parameters.protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        return StatusCode(await extproto.ExternalMessageUpdate(parameters.message, parameters.authorExternalId, parameters.channelExternalId));
    }
    ///<summary>"created" as far as our bookkeeping is concerned. "first spotted" would be valid. etc.</summary>
    [HttpPost]
    [Route("AccountCreated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AccountCreated(string protocolExternalId, Account account, string channelExternalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        return StatusCode(await extproto.ExternalAccountCreate(account, channelExternalId));
    }
    [HttpPost]
    [Route("AccountUpdated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AccountUpdated(string protocolExternalId, Account account, string channelExternalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        return StatusCode(await extproto.ExternalAccountUpdate(account, channelExternalId));
    }
    ///<summary>"created" as far as our bookkeeping is concerned. "first spotted" would be valid. etc.</summary>
    [HttpPost]
    [Route("ChannelCreated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<IActionResult> ChannelCreated(string protocolExternalId, Channel channel, List<string> channelLineage)
    public async Task<IActionResult> ChannelCreated(Tuple<string, Channel, List<string>> parameters)
    {
        string protocolExternalId = parameters.Item1;
        Channel channel = parameters.Item2;
        List<string> channelLineage = parameters.Item3;
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        return StatusCode(await extproto.ExternalChannelJoin(channel, channelLineage));
    }

    [HttpPost]
    [Route("ChannelUpdated")]
    [Produces("application/json")]
    // public async Task<IActionResult> ChannelUpdated(string protocolExternalId, Channel channel, List<string> channelLineage) //s2g sometimes that works and sometimes it doesn't
    public async Task<IActionResult> ChannelUpdated(Tuple<string, Channel, List<string>> parameters)
    {
        string protocolExternalId = parameters.Item1;
        Channel channel = parameters.Item2;
        List<string> channelLineage = parameters.Item3;

        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        return StatusCode(await extproto.ExternalChannelUpdate(channel, channelLineage));
    }
}
