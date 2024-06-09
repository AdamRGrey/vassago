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

	[HttpPost]
	[Produces("application/json")]
	public Channel Post([FromBody] Channel channel)
	{
		// Write logic to insert employee data
		return new Channel();
	}
}