namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Protocol : Channel
{
    //log in, log out, observe events?

    //doesn't actually have to be a token, but it should be how an interface can find itself in the DB
    public string ConnectionToken { get; set; }
}
