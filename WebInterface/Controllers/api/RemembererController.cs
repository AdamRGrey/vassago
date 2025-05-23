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
        return Rememberer.AccountDetail(id);
    }
    [HttpPut]
    [Route("Attachment")]
    [Produces("application/json")]
    public Attachment CreateAttachment(Guid id)
    {
        return Rememberer.AttachmentDetail(id);
    }
    [HttpPut]
    [Route("Channel")]
    [Produces("application/json")]
    public Channel CreateChannel(Guid id)
    {
        return Rememberer.ChannelDetail(id);
    }
    [HttpPut]
    [Route("Message")]
    [Produces("application/json")]
    public Message CreateMessage(Guid id)
    {
        return Rememberer.MessageDetail(id);
    }
    [HttpPut]
    [Route("UAC")]
    [Produces("application/json")]
    public UAC CreateUAC(Guid id)
    {
        return Rememberer.UACDetail(id);
    }
    [HttpPut]
    [Route("User")]
    [Produces("application/json")]
    public User CreateUser(Guid id)
    {
        return Rememberer.UserDetail(id);
    }
    //Read
    [HttpGet]
    [Route("Account")]
    [Produces("application/json")]
    public Account GetAccount(Guid id)
    {
        return Rememberer.AccountDetail(id);
    }
    [HttpGet]
    [Route("Attachment")]
    [Produces("application/json")]
    public Attachment GetAttachment(Guid id)
    {
        return Rememberer.AttachmentDetail(id);
    }
    [HttpGet]
    [Route("Channel")]
    [Produces("application/json")]
    public Channel GetChannel(Guid id)
    {
        return Rememberer.ChannelDetail(id);
    }
    [HttpGet]
    [Route("Message")]
    [Produces("application/json")]
    public Message GetMessage(Guid id)
    {
        return Rememberer.MessageDetail(id);
    }
    [HttpGet]
    [Route("UAC")]
    [Produces("application/json")]
    public UAC GetUAC(Guid id)
    {
        return Rememberer.UACDetail(id);
    }
    [HttpGet]
    [Route("User")]
    [Produces("application/json")]
    public User GetUser(Guid id)
    {
        return Rememberer.UserDetail(id);
    }
    //Update
    [HttpPatch]
    [Route("Channel")]
    [Produces("application/json")]
    public IActionResult Patch([FromBody] Channel channel)
    {
        var fromDb = Rememberer.ChannelDetail(channel.Id);
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
        Rememberer.RememberChannel(fromDb);
        return Ok(fromDb);
    }
    //Delete
    [HttpDelete]
    [Route("Account")]
    [Produces("application/json")]
    public IActionResult DeleteAccount(Guid id)
    {
        var fromDb = Rememberer.AccountDetail(id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete account {id}, not found");
            return NotFound();
        }
        Rememberer.ForgetAccount(fromDb);
        return Ok();
    }
    [HttpDelete]
    [Route("Attachment")]
    [Produces("application/json")]
    public IActionResult DeleteAttachment(Guid id)
    {
        var fromDb = Rememberer.AttachmentDetail(id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete attachment {id}, not found");
            return NotFound();
        }
        Rememberer.ForgetAttachment(fromDb);
        return Ok();
    }
    [HttpDelete]
    [Route("Channel")]
    [Produces("application/json")]
    public IActionResult DeleteChannel(Guid id)
    {
        var fromDb = Rememberer.ChannelDetail(id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete channel {id}, not found");
            return NotFound();
        }
        Rememberer.ForgetChannel(fromDb);
        return Ok();
    }
    [HttpDelete]
    [Route("Message")]
    [Produces("application/json")]
    public IActionResult DeleteMessage(Guid id)
    {
        var fromDb = Rememberer.MessageDetail(id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete message {id}, not found");
            return NotFound();
        }
        Rememberer.ForgetMessage(fromDb);
        return Ok();
    }
    [HttpDelete]
    [Route("UAC")]
    [Produces("application/json")]
    public IActionResult DeleteUAC(Guid id)
    {
        var fromDb = Rememberer.UACDetail(id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete uac {id}, not found");
            return NotFound();
        }
        Rememberer.ForgetUAC(fromDb);
        return Ok();
    }
    [HttpDelete]
    [Route("User")]
    [Produces("application/json")]
    public IActionResult DeleteUser(Guid id)
    {
        var fromDb = Rememberer.UserDetail(id);
        if (fromDb == null)
        {
            _logger.LogError($"attempt to delete user {id}, not found");
            return NotFound();
        }
        Rememberer.ForgetUser(fromDb);
        return Ok();
    }
}
