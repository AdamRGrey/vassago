namespace vassago.Models;

using Microsoft.EntityFrameworkCore;

public class ChattingContext : DbContext
{
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Channel> Channels { get; set; }
    //public DbSet<Emoji> Emoji {get;set;}
    public DbSet<Message> Messages { get; set; }
    public DbSet<PermissionSettings> PermissionSettings{get;set;}
    public DbSet<Protocol> Protocols { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(Shared.DBConnectionString);
}