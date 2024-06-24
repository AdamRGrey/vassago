using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;

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
			_logger.LogError($"attempt to update channel {channel.Id}, not found"); //ca2254 is moronic. maybe if it wasn't filed under "code quality" and instead was filed under "you didn't include a workaround for the weaknesses of other external junk" i'd be kinder to it ;)
			return NotFound();
        }
		//settable values: lewdness filter level, meanness filter level. maybe i could decorate them... 
		fromDb.LewdnessFilterLevel = channel.LewdnessFilterLevel;
		fromDb.MeannessFilterLevel = channel.MeannessFilterLevel;
		_db.SaveChanges();
        return Ok(fromDb);
    }
}
