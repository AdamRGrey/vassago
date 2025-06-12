using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.ProtocolInterfaces.DiscordInterface;

namespace vassago.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class UACController : ControllerBase
{
    private readonly ILogger<UACController> _logger;

    public UACController(ILogger<UACController> logger)
    {
        _logger = logger;
    }

    //microsoft: "you can't have multiple [FromBody]. The reason for this rule is some bullshti about storage buffers."
    //cool story, bro. nobody gives a fuck, look at the boilerplate you've necessitated.
    public class extraSpecialObjectReadGlorifiedTupleFor_LinkChannel
    {
        public Guid uac_guid;
        public Guid channel_guid;
    }
    [HttpPatch]
    [Route("LinkChannel")]
    [Produces("application/json")]
    public IActionResult LinkChannel([FromBody] extraSpecialObjectReadGlorifiedTupleFor_LinkChannel req)
    {
        var uac_guid = req.uac_guid;
        var channel_guid = req.channel_guid;
        var uacFromDb = Rememberer.SearchUAC(uac => uac.Id == uac_guid);
        if (uacFromDb == null)
        {
            var err = $"attempt to link channel for uac {uac_guid}, not found";
            _logger.LogError(err);
            return NotFound(err);
        }
        var channelFromDb = Rememberer.SearchChannel(c => c.Id == channel_guid);
        if (channelFromDb == null)
        {
            var err = $"attempt to link channel for channel {channel_guid}, not found";
            _logger.LogError(err);
            return NotFound(err);
        }

        uacFromDb.Channels ??= [];
        if (uacFromDb.Channels.Contains(channelFromDb))
        {
            return BadRequest("channel already linked");
        }
        uacFromDb.Channels.Add(channelFromDb);
        Rememberer.RememberUAC(uacFromDb);
        return Ok(uacFromDb);
    }
    public class extraSpecialObjectReadGlorifiedTupleFor_LinkUser
    {
        public Guid uac_guid;
        public Guid user_guid;
    }
    [HttpPatch]
    [Route("LinkUser")]
    [Produces("application/json")]
    public IActionResult LinkUser([FromBody] extraSpecialObjectReadGlorifiedTupleFor_LinkUser req)
    {
        var uac_guid = req.uac_guid;
        var user_guid = req.user_guid;
        var uacFromDb = Rememberer.SearchUAC(uac => uac.Id == uac_guid);
        if (uacFromDb == null)
        {
            _logger.LogError($"attempt to link channal for uac {uac_guid}, not found");
            return NotFound();
        }
        var userFromDb = Rememberer.SearchUser(c => c.Id == user_guid);
        if (userFromDb == null)
        {
            _logger.LogError($"attempt to link user for user {user_guid}, not found");
            return NotFound();
        }

        uacFromDb.Users ??= [];
        if (uacFromDb.Users.Contains(userFromDb))
        {
            return BadRequest("user already linked");
        }
        uacFromDb.Users.Add(userFromDb);
        Rememberer.RememberUAC(uacFromDb);
        return Ok(uacFromDb);
    }
    public class extraSpecialObjectReadGlorifiedTupleFor_LinkAccount
    {
        public Guid uac_guid;
        public Guid account_guid;
    }
    [HttpPatch]
    [Route("LinkAccount")]
    [Produces("application/json")]
    public IActionResult LinkAccount([FromBody] extraSpecialObjectReadGlorifiedTupleFor_LinkAccount req)
    {
        var uac_guid = req.uac_guid;
        var account_guid = req.account_guid;
        var uacFromDb = Rememberer.SearchUAC(uac => uac.Id == uac_guid);
        if (uacFromDb == null)
        {
            _logger.LogError($"attempt to link channal for uac {uac_guid}, not found");
            return NotFound();
        }
        var accountFromDb = Rememberer.SearchAccount(c => c.Id == account_guid);
        if (accountFromDb == null)
        {
            _logger.LogError($"attempt to link account for user {account_guid}, not found");
            return NotFound();
        }

        uacFromDb.AccountInChannels ??= [];
        if (uacFromDb.AccountInChannels.Contains(accountFromDb))
        {
            return BadRequest("account already linked");
        }
        uacFromDb.AccountInChannels.Add(accountFromDb);
        Rememberer.RememberUAC(uacFromDb);
        return Ok(uacFromDb);
    }
    [HttpPatch]
    [Route("UnlinkUser")]
    [Produces("application/json")]
    public IActionResult UnlinkUser([FromBody] extraSpecialObjectReadGlorifiedTupleFor_LinkUser req)
    {
        var uac_guid = req.uac_guid;
        var user_guid = req.user_guid;
        var uacFromDb = Rememberer.SearchUAC(uac => uac.Id == uac_guid);
        if (uacFromDb == null)
        {
            _logger.LogError($"attempt to unlink uac for uac {uac_guid}, not found");
            return NotFound();
        }
        var userFromDb = Rememberer.SearchUser(c => c.Id == user_guid);
        if (userFromDb == null)
        {
            _logger.LogError($"attempt to unlink user for user {user_guid}, not found");
            return NotFound();
        }

        uacFromDb.Users ??= [];
        if (!uacFromDb.Users.Contains(userFromDb))
        {
            return BadRequest("user not linked");
        }
        uacFromDb.Users.Remove(userFromDb);
        Rememberer.RememberUAC(uacFromDb);
        return Ok(uacFromDb);
    }
    [HttpPatch]
    [Route("UnlinkAccount")]
    [Produces("application/json")]
    public IActionResult UnlinkAccount([FromBody] extraSpecialObjectReadGlorifiedTupleFor_LinkAccount req)
    {
        var uac_guid = req.uac_guid;
        var account_guid = req.account_guid;
        var uacFromDb = Rememberer.SearchUAC(uac => uac.Id == uac_guid);
        if (uacFromDb == null)
        {
            _logger.LogError($"attempt to unlink uac for uac {uac_guid}, not found");
            return NotFound();
        }
        var accountFromDb = Rememberer.SearchAccount(a => a.Id == account_guid);
        if (accountFromDb == null)
        {
            _logger.LogError($"attempt to unlink account for user {account_guid}, not found");
            return NotFound();
        }

        uacFromDb.AccountInChannels ??= [];
        if (!uacFromDb.AccountInChannels.Contains(accountFromDb))
        {
            return BadRequest("account not linked");
        }
        uacFromDb.AccountInChannels.Remove(accountFromDb);
        Rememberer.RememberUAC(uacFromDb);
        return Ok(uacFromDb);
    }
    [HttpPatch]
    [Route("UnlinkChannel")]
    [Produces("application/json")]
    public IActionResult UnlinkChannel([FromBody] extraSpecialObjectReadGlorifiedTupleFor_LinkChannel req)
    {
        var uac_guid = req.uac_guid;
        var channel_guid = req.channel_guid;
        var uacFromDb = Rememberer.SearchUAC(uac => uac.Id == uac_guid);
        if (uacFromDb == null)
        {
            _logger.LogError($"attempt to unlink channal for uac {uac_guid}, not found");
            return NotFound();
        }
        var channelFromDb = Rememberer.SearchChannel(c => c.Id == channel_guid);
        if (channelFromDb == null)
        {
            _logger.LogError($"attempt to unlink user for user {channel_guid}, not found");
            return NotFound();
        }

        uacFromDb.Users ??= [];
        if (!uacFromDb.Channels.Contains(channelFromDb))
        {
            return BadRequest("user not linked");
        }
        uacFromDb.Channels.Remove(channelFromDb);
        Rememberer.RememberUAC(uacFromDb);
        return Ok(uacFromDb);
    }
    [HttpPut]
    [Route("CreateForChannels/{Id}")]
    [Produces("application/json")]
    public IActionResult CreateForChannels(Guid Id)
    {
        _logger.LogDebug($"made it to controller. creating for channel {Id}");
        var targetChannel = Rememberer.ChannelDetail(Id);
        if (targetChannel == null)
        {
            return NotFound();
        }
        var newUAC = new UAC()
        {
            Channels = [targetChannel]
        };
        Rememberer.RememberUAC(newUAC);
        Rememberer.RememberChannel(targetChannel);
        return Ok(newUAC.Id);
    }
}
