namespace vassago.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

public class ChattingContext : DbContext
{
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<UAC> UACs { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Configuration> Configurations {get; set;}
    public DbSet<Webhook> Webhooks {get; set;}

    public ChattingContext(DbContextOptions<ChattingContext> options) : base(options) { }
    public ChattingContext() : base() { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(Shared.DBConnectionString);
            //.EnableSensitiveDataLogging(true); //"sensitive" is one thing. writing "did something" every time you think a thought is a different, stupid thing.
    }
}
