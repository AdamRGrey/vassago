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
    public Guid Create(Joke joke)
    {
        r.RememberJoke(joke);
        return joke.Id;
    }
    [HttpGet]
    [Route("GetSpecific")]
    public Joke Get(Guid id)
    {
        return r.SearchJoke(id);
    }
    [HttpPatch]
    [Route("Update")]
    public Joke Update(Joke joke)
    {
        var updated = r.SearchJoke(joke.Id);
        updated.PrimaryText = joke.PrimaryText;
        updated.SecondaryText= joke.SecondaryText;
        updated.LewdnessConformity = joke.LewdnessConformity;
        updated.MeannessConformity= joke.MeannessConformity;
        r.RememberJoke(updated);
        return updated;
    }
    [HttpDelete]
    [Route("Delete")]
    public void Delete(Guid id)
    {
        r.ForgetJoke(id);
    }
}
