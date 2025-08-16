using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vassago.Models;
using vassago.ProtocolInterfaces;

namespace vassago.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private static Rememberer r = Rememberer.Instance;

    public AccountsController(ILogger<AccountsController> logger)
    {
        _logger = logger;
    }

    //microsoft: "you can't have multiple [FromBody]. The reason for this rule is some bullshti about storage buffers."
    //cool story, bro. nobody gives a fuck, look at the boilerplate you've necessitated.
    // OH. this guy clarifies it better.  - https://stackoverflow.com/a/31686405 web api uses the content-type header to pick a converter.
    // so if it sees [frombody] complextype1 foo, string bar, [frombody] complextype2 baz, it doesn't want to guess too much about how to intake all that in one body.
    // ...even though, obviously, it would be a tuple. what else would it be? if I had only native types, you'd make that assumption!
    //psst, self: go look at external protocol controller, there's much nicer ways to make workaround-tuples like this.
    public class extraSpecialObjectReadGlorifiedTupleFor_UnlinkUser
    {
        public Guid acc_guid;
    }
    [HttpPatch]
    [Route("UnlinkUser")]
    [Produces("application/json")]
    public IActionResult UnlinkUser([FromBody] extraSpecialObjectReadGlorifiedTupleFor_UnlinkUser req)
    {
        var acc_guid = req.acc_guid;
        var accFromDb = r.SearchAccount(acc => acc.Id == acc_guid);
        if (accFromDb == null)
        {
            var err = $"attempt to unlink user for acc {acc_guid}, not found";
            _logger.LogError(err);
            return NotFound(err);
        }
        var userFromDb = r.SearchUser(c => c.Id == accFromDb.IsUser.Id);
        if (userFromDb == null)
        {
            var err = $"attempt to unlink user for {acc_guid}, doesn't have a user";
            _logger.LogError(err);
            return NotFound(err);
        }

        accFromDb.IsUser = null;

        r.RememberAccount(accFromDb);
        return Ok(accFromDb);
    }
}
