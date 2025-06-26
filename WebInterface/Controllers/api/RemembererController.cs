using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;

namespace vassago.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class RemembererController : ControllerBase
{
    private readonly ILogger<RemembererController> _logger;
    private static Rememberer r = Rememberer.Instance;

    public RemembererController(ILogger<RemembererController> logger)
    {
        _logger = logger;
    }

    //Create
    [HttpPut]
    [Route("Account")]
    [Produces("application/json")]
    public Account CreateAccount(Guid id)
    {
        return r.AccountDetail(id);
    }
    [HttpPut]
    [Route("Attachment")]
    [Produces("application/json")]
    public Attachment CreateAttachment(Guid id)
    {
        return r.AttachmentDetail(id);
    }
    [HttpPut]
    [Route("Channels")]
    [Produces("application/json")]
    public Channel CreateChannel(Guid id)
    {
        return r.ChannelDetail(id);
    }
    [HttpPut]
    [Route("Message")]
    [Produces("application/json")]
    public Message CreateMessage(Guid id)
    {
        return r.MessageDetail(id);
    }
    [HttpPut]
    [Route("UAC")]
    [Produces("application/json")]
    public UAC CreateUAC(Guid id)
    {
        return r.UACDetail(id);
    }
    [HttpPut]
    [Route("User")]
    [Produces("application/json")]
    public User CreateUser(Guid id)
    {
        return r.UserDetail(id);
    }
    //Read
    [HttpGet]
    [Route("Account")]
    [Produces("application/json")]
    public Account GetAccount(Guid id)
    {
        return r.AccountDetail(id);
    }
    [HttpGet]
    [Route("Attachment")]
    [Produces("application/json")]
    public Attachment GetAttachment(Guid id)
    {
        return r.AttachmentDetail(id);
    }
    [HttpGet]
    [Route("Channels")]
    [Produces("application/json")]
    public Channel GetChannel(Guid id)
    {
        return r.ChannelDetail(id);
    }
    [HttpGet]
    [Route("Message")]
    [Produces("application/json")]
    public Message GetMessage(Guid id)
    {
        return r.MessageDetail(id);
    }
    [HttpGet]
    [Route("UAC")]
    [Produces("application/json")]
    public UAC GetUAC(Guid id)
    {
        return r.UACDetail(id);
    }
    [HttpGet]
    [Route("User")]
    [Produces("application/json")]
    public User GetUser(Guid id)
    {
        return r.UserDetail(id);
    }
    //Update
    [HttpPatch]
    [Route("Channels")]
    [Produces("application/json")]
    public IActionResult Patch([FromBody] Channel channel)
    {
        var fromDb = r.ChannelDetail(channel.Id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to update channel {channel.Id}, not found");
            return NotFound();
        }
        else
        {
            _logger.LogDebug($"patching {channel.DisplayName} (id: {channel.Id})");
        }
        //settable values: lewdness filter level, meanness filter level. maybe i could decorate them...
        fromDb.LewdnessFilterLevel = channel.LewdnessFilterLevel;
        fromDb.MeannessFilterLevel = channel.MeannessFilterLevel;
        r.RememberChannel(fromDb);
        return Ok(fromDb);
    }
    //Delete
    [HttpDelete]
    [Route("Account")]
    [Produces("application/json")]
    public IActionResult DeleteAccount(Guid id)
    {
        var fromDb = r.AccountDetail(id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete account {id}, not found");
            return NotFound();
        }
        r.ForgetAccount(fromDb);
        return Ok();
    }
    [HttpDelete]
    [Route("Attachment")]
    [Produces("application/json")]
    public IActionResult DeleteAttachment(Guid id)
    {
        var fromDb = r.AttachmentDetail(id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete attachment {id}, not found");
            return NotFound();
        }
        r.ForgetAttachment(fromDb);
        return Ok();
    }
    [HttpDelete]
    [Route("Channels/{id}")]
    [Produces("application/json")]
    public IActionResult DeleteChannel(Guid id)
    {
        var fromDb = r.ChannelDetail(id);
        _logger.LogDebug($"delete channel {id}");
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete channel {id}, not found");
            return NotFound();
        }
        r.ForgetChannel(fromDb);
        _logger.LogDebug($"delete channel {id} success");
        return Ok();
    }
    [HttpDelete]
    [Route("Message/{id}")]
    [Produces("application/json")]
    public IActionResult DeleteMessage(Guid id)
    {
        var fromDb = r.MessageDetail(id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete message {id}, not found");
            return NotFound();
        }
        r.ForgetMessage(fromDb);
        return Ok();
    }
    [HttpDelete]
    [Route("UAC/{id}")]
    [Produces("application/json")]
    public IActionResult DeleteUAC(Guid id)
    {
        var fromDb = r.UACDetail(id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete uac {id}, not found");
            return NotFound();
        }
        r.ForgetUAC(fromDb);
        return Ok();
    }
    [HttpDelete]
    [Route("User/{id}")]
    [Produces("application/json")]
    public IActionResult DeleteUser(Guid id)
    {
        var fromDb = r.UserDetail(id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete user {id}, not found");
            return NotFound();
        }
        r.ForgetUser(fromDb);
        return Ok();
    }
}
