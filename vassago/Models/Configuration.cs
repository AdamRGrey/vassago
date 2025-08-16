namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using vassago.ProtocolInterfaces;
using vassago.Behavior;

//it feels gross to have a *table* in a database that's intended to hold 1 **UND EXACTLY ONE** row, ever.
//but also it feels worse to scatter my configuraiton-y data across external files and the database.

public class Configuration
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string ExchangePairsLocation { get; set; } = "assets/exchangepairs.json"; //TODO: have this be "exchange API key", so you can have it continually update.
    public string KafkaBootstrap { get; set; } = "http://localhost:9092";
    public string KafkaName { get; set; } = "vassago";
    public string reportedApiUrl { get; set; } = "http://localhost:5093/api";
}
