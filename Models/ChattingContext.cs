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
    public DbSet<Configuration> Configurations { get; set; }
    public DbSet<Webhook> Webhooks { get; set; }
    public DbSet<ProtocolConfiguration> ProtocolConfigurations { get; set; }
    public DbSet<ProtocolTwitch> ProtocolTwitchs { get; set; }
    public DbSet<ProtocolDiscord> ProtocolDiscords { get; set; }
    public DbSet<ProtocolExternal> ProtocolExternals { get; set; }

    public ChattingContext(DbContextOptions<ChattingContext> options) : base(options) { }
    public ChattingContext() : base() { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(Shared.DBConnectionString);
        //.EnableSensitiveDataLogging(true); //logging "sensitive" data (i.e., information that might be useful for debugging) is one thing.
        //writing "did something" every time you think a thought is a different, retarded thing that no one wants.
        //https://www.reddit.com/r/dotnet/comments/1ctr95j/entity_framework_core_logging/ <-- i'm pretty sure that thread talks about it,
        //but reddit thinks i'm someone else's robot so no access for me
    }
}
