namespace vassago.Controllers.api;

using Microsoft.AspNetCore.Mvc;
using vassago.Models;
using vassago.ProtocolInterfaces;
using Newtonsoft.Json;
using static vassago.Models.Enumerations;

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
    [HttpGet]
    [Route("GetChannel")]
    public IActionResult GetChannel(string protocolExternalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        return Ok(extproto.SelfChannel);
    }
    ///<summary>for the first time. reconnecting, you're on your own... until I do the Webhook and other style. All we want is External and Style</summary>
    [HttpPost]
    [Route("Connect")]
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
            confEntity = new ProtocolExternal()
            {
                ExternalId = incoming.ExternalId,
                Style = ExternalProtocolStyle.Restful
            };
            r.RememberExternal(confEntity);
            try
            {
                await Reconfigurator.ProtocolInterfaces();
                extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == incoming.ExternalId)
                    as ExternalRestful;
                return Ok(extproto.SelfChannel);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[connect] tried. failed.");
                Console.Error.WriteLine(e);
                r.ForgetExternal(incoming.Id);
                await Reconfigurator.ProtocolInterfaces();
                Console.WriteLine($"[connect] survived reconfigurating. return error.");
                return BadRequest(e);
            }
        }
        else
        {
            var oldConf = JsonConvert.DeserializeObject<ProtocolExternal>(JsonConvert.SerializeObject(confEntity));
            Console.WriteLine($"[connect] found old");
            if (confEntity.Style != incoming.Style)
            {
                confEntity.Style = incoming.Style;
                r.RememberExternal(confEntity);
                try
                {
                    Console.WriteLine($"[connect] found old");
                    await Reconfigurator.ProtocolInterfaces();
                    Console.WriteLine($"[connect] survived reconfigurating.");
                    extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == incoming.ExternalId)
                        as ExternalRestful;
                    Console.WriteLine($"[connect] we'll be returning one. not null: {extproto != null}");
                    return Ok(extproto.SelfChannel);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[connect] tried. failed.");
                    r.ForgetExternal(incoming.Id);
                    Console.WriteLine($"[connect] survived forgetting.");
                    r.RememberExternal(oldConf);
                    Console.WriteLine($"[connect] survived re-remembering old version.");
                    await Reconfigurator.ProtocolInterfaces();
                    Console.WriteLine($"[connect] survived reconfigurating with old version.");
                    return BadRequest(e);
                }
            }
            Console.WriteLine("[connect] no changes. status code two hundred ellipsis question mark: ok...?");
            return Ok(extproto.SelfChannel);
        }
    }

    ///<summary>if you do not intend to reconnect. when (read: if ever) i do webhook style, i'll want a temporary disconnect notification</summary>
    [HttpPost]
    [Route("Disconnect")]
    public async Task<IActionResult> Disconnect(string externalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == externalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        r.ForgetExternal(extproto.ConfigurationEntity.Id);
        Reconfigurator.ProtocolInterfaces();

        return Ok();
    }
    [HttpPost]
    [Route("MessageReceived")]
    public async Task<IActionResult> MessageReceived(string protocolExternalId, Message message, string authorExternalId, string channelExternalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        return StatusCode(await extproto.ExternalMessageReceive(message, authorExternalId, channelExternalId));
    }
    [HttpPost]
    [Route("MessageUpdated")]
    public async Task<IActionResult> MessageUpdated(string protocolExternalId, Message message, string authorExternalId, string channelExternalId)
    {
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        return StatusCode(await extproto.ExternalMessageUpdate(message, authorExternalId, channelExternalId));
    }
    ///<summary>"created" as far as our bookkeeping is concerned. "first spotted" would be valid. etc.</summary>
    [HttpPost]
    [Route("AccountCreated")]
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
    public async Task<IActionResult> ChannelCreated(Tuple<string, Channel, string> parameters)
    {
        string protocolExternalId = parameters.Item1;
        Channel channel = parameters.Item2;
        string parentChannelId = parameters.Item3;
        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
            return NotFound();

        return StatusCode(await extproto.ExternalChannelJoin(channel, parentChannelId));
    }

    [HttpPost]
    [Route("ChannelUpdated")]
    public async Task<IActionResult> ChannelUpdated(Tuple<string, Channel, string> parameters)
    {
        //Console.WriteLine($"[api ChannelUpdated]");
        string protocolExternalId = parameters.Item1;
        Channel channel = parameters.Item2;
        string parentChannelId = parameters.Item3;
        //Console.WriteLine($"[api ChannelUpdated] - present? {!string.IsNullOrWhiteSpace(protocolExternalId)}, {channel != null}, {!string.IsNullOrWhiteSpace(parentChannelId)}");

        var extproto = Shared.ProtocolList.FirstOrDefault(p => (p as ExternalRestful) != null && (p as ExternalRestful).SelfChannel.ExternalId == protocolExternalId)
            as ExternalRestful;
        if (extproto == null)
        {
            Console.Error.WriteLine($"[api ChannelUpdate] = couldn't find external protocol handler for {protocolExternalId}");
            return NotFound();
        }
        //Console.WriteLine($"[api ChannelUpdated] - looks good, shoving off on the protocol handler object");

        return StatusCode(await extproto.ExternalChannelUpdate(channel, parentChannelId));
    }

    [HttpPost]
    [Route("ValidateChannel")]
    public async Task<bool> ValidateChannel(Channel param)
    {
        if(param != null)
            return true;

        return false;
    }
}
