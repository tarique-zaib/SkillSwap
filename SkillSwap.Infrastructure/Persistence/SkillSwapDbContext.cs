using Microsoft.EntityFrameworkCore;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Infrastructure.Persistence;

public class SkillSwapDbContext : DbContext
{
    public SkillSwapDbContext(
        DbContextOptions<SkillSwapDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<SkillListing> SkillListings => Set<SkillListing>();
    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Favor> Favors => Set<Favor>();
    public DbSet<Vouch> Vouches => Set<Vouch>();
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>()
    .HasOne(x => x.User)
    .WithMany()
    .HasForeignKey(x => x.UserId)
    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SkillSwapDbContext).Assembly);
    }
}