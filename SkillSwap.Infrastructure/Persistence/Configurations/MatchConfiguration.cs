using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.OfferId,
            x.NeedId
        })
        .IsUnique();

        builder.HasOne(x => x.Offer)
            .WithMany()
            .HasForeignKey(x => x.OfferId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Need)
            .WithMany()
            .HasForeignKey(x => x.NeedId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}