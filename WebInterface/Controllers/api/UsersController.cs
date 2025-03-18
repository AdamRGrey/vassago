using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.ProtocolInterfaces.DiscordInterface;

namespace vassago.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ILogger<ChannelsController> _logger;

    public UsersController(ILogger<ChannelsController> logger)
    {
        _logger = logger;
    }

    [HttpPatch]
    [Produces("application/json")]
    public IActionResult Patch([FromBody] User user)
    {
        var fromDb = Rememberer.SearchUser(u => u.Id == user.Id);
        if (fromDb == null)
		{
			_logger.LogError($"attempt to update user {user.Id}, not found");
			return NotFound();
        }
        else
        {
            _logger.LogDebug($"patching {user.DisplayName} (id: {user.Id})");
        }

        //TODO: settable values: display name 
		//fromDb.DisplayName = user.DisplayName;
		Rememberer.RememberUser(fromDb);
        return Ok(fromDb);
    }
}