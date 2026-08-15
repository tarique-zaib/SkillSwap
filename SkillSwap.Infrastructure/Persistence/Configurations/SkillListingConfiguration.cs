using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Infrastructure.Persistence.Configurations;

public class SkillListingConfiguration
    : IEntityTypeConfiguration<SkillListing>
{
    public void Configure(EntityTypeBuilder<SkillListing> builder)
    {
        builder.ToTable("skill_listings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.Category,
            x.Type,
            x.IsActive
        });

        builder.HasIndex(x => x.CreatedAtUtc);
    }
}