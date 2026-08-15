using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Infrastructure.Persistence.Configurations;

public class FavorConfiguration : IEntityTypeConfiguration<Favor>
{
    public void Configure(EntityTypeBuilder<Favor> builder)
    {
        builder.ToTable("favors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.MatchId)
            .IsUnique();

        builder.HasOne(x => x.Match)
            .WithOne()
            .HasForeignKey<Favor>(x => x.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}