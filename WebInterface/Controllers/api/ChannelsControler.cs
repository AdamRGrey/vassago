using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.ProtocolInterfaces.DiscordInterface;

namespace vassago.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class ChannelsController : ControllerBase
{
    private readonly ILogger<ChannelsController> _logger;
    private readonly ChattingContext _db;

    public ChannelsController(ILogger<ChannelsController> logger, ChattingContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet("{id}")]
    [Produces("application/json")]
    public Channel Get(Guid id)
    {
        return _db.Find<Channel>(id);
    }

    [HttpPatch]
    [Produces("application/json")]
    public IActionResult Patch([FromBody] Channel channel)
    {
        var fromDb = _db.Channels.Find(channel.Id);
        if (fromDb == null)
		{
			_logger.LogError($"attempt to update channel {channel.Id}, not found");
			return NotFound();
        }
		//settable values: lewdness filter level, meanness filter level. maybe i could decorate them... 
		fromDb.LewdnessFilterLevel = channel.LewdnessFilterLevel;
		fromDb.MeannessFilterLevel = channel.MeannessFilterLevel;
		_db.SaveChanges();
        return Ok(fromDb);
    }
    [HttpDelete]
    [Produces("application/json")]
    public IActionResult Delete([FromBody] Channel channel)
    {
        var fromDb = _db.Channels.Find(channel.Id);
        if (fromDb == null)
		{
			_logger.LogError($"attempt to delete channel {channel.Id}, not found");
			return NotFound();
        }
        deleteChannel(fromDb);
        _db.SaveChanges();
        return Ok();
    }
    private void deleteChannel(Channel channel)
    {
        if (channel.SubChannels?.Count > 0)
        {
            foreach (var childChannel in channel.SubChannels)
            {
                deleteChannel(childChannel);
            }
        }

        if(channel.Users?.Count > 0)
        {
            foreach(var account in channel.Users)
            {
                deleteAccount(account);
            }
        }

        if(channel.Messages?.Count > 0)
        {
            _db.Remove(channel.Messages);
        }

        _db.Remove(channel);
    }
    private void deleteAccount(Account account)
    {
        var user = account.IsUser;
        var usersOnlyAccount = user.Accounts?.Count == 1;
        
        _db.Remove(account);
        
        if(usersOnlyAccount)
            _db.Users.Remove(user);
    }
}
