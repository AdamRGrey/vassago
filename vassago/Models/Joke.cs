namespace vassago.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using static vassago.Models.Enumerations;

public class Joke
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public LewdnessFilterLevel LewdnessConformity { get; set; }
    public MeannessFilterLevel MeannessConformity { get; set; }
    public string PrimaryText { get; set; }
    public string SecondaryText { get; set; }
}
