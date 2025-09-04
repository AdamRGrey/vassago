using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.ProtocolInterfaces;

namespace vassago.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class JokesController : ControllerBase
{
    private readonly ILogger<JokesController > _logger;
    private static Rememberer r = Rememberer.Instance;

    public JokesController(ILogger<JokesController > logger)
    {
        _logger = logger;
    }
    [HttpPut]
    [Route("Create")]
    public void Create(Joke joke)
    {
        r.RememberJoke(joke);
    }
    [HttpGet]
    [Route("GetSpecific")]
    public Joke Get(Guid id)
    {
        return r.SearchJoke(id);
    }
    [HttpPatch]
    [Route("Update")]
    public void Update(Joke joke)
    {
        r.RememberJoke(joke);
    }
    [HttpDelete]
    [Route("Delete")]
    public void Delete(Guid id)
    {
        r.ForgetJoke(id);
    }
}
