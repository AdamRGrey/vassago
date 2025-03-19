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

    public ChannelsController(ILogger<ChannelsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{id}")]
    [Produces("application/json")]
    public Channel Get(Guid id)
    {
        return Rememberer.ChannelDetail(id);
    }

    [HttpPatch]
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
    [HttpDelete]
    [Produces("application/json")]
    public IActionResult Delete([FromBody] Channel channel)
    {
        var fromDb = Rememberer.ChannelDetail(channel.Id);
        if (fromDb == null)
		{
			_logger.LogError($"attempt to delete channel {channel.Id}, not found");
			return NotFound();
        }
        deleteChannel(fromDb);
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

        Rememberer.ForgetChannel(channel);
    }
    private void deleteAccount(Account account)
    {
        var user = account.IsUser;
        var usersOnlyAccount = user.Accounts?.Count == 1;
        
        Rememberer.ForgetAccount(account);
        
        if(usersOnlyAccount)
            Rememberer.ForgetUser(user);
    }
}
