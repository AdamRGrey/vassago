namespace vassago.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

public class ChattingContext : DbContext
{
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Channel> Channels { get; set; }
    //public DbSet<Emoji> Emoji {get;set;}
    public DbSet<Message> Messages { get; set; }
    public DbSet<PermissionSettings> PermissionSettings{get;set;}
    public DbSet<Account> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(Shared.DBConnectionString)
            .EnableSensitiveDataLogging(true); //who the fuck is looking at log output but not allowed to see it? this should be on by default.
}