namespace vassago.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using vassago.TwitchInterface;
using vassago.Behavior;

//TODO: it feels gross to have a *table* in a database that's intended to hold 1 **UND EXACTLY ONE** row, ever.
//but also it feels worse to scatter my configuraiton-y data across external files and the database.

public class Configuration
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public List<string> DiscordTokens { get; set; }
    public List<string> TwitchConfigs { get; set; }
    public string ExchangePairsLocation { get; set; } = "assets/exchangepairs.json"; //TODO: have this be "exchange API key", so you can have it continually update.
    public bool SetupDiscordSlashCommands { get; set; } = false; //i'm kind of idealogically opposed to these.
    public List<string> Webhooks { get; set; }
    public string KafkaBootstrap { get; set; } = "http://localhost:9092";
    public string KafkaName { get; set; } = "vassago";
    public string reportedApiUrl { get; set; } = "http://localhost:5093/api";
}
