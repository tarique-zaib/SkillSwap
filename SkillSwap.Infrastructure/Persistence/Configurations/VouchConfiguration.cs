using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Infrastructure.Persistence.Configurations;

public class VouchConfiguration : IEntityTypeConfiguration<Vouch>
{
    public void Configure(EntityTypeBuilder<Vouch> builder)
    {
        builder.ToTable("vouches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SkillCategory)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.StoryText)
            .HasMaxLength(2000);

        builder.HasIndex(x => new
        {
            x.FavorId,
            x.FromUserId,
            x.ToUserId
        })
        .IsUnique();

        builder.HasOne(x => x.FromUser)
            .WithMany()
            .HasForeignKey(x => x.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToUser)
            .WithMany()
            .HasForeignKey(x => x.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Favor)
            .WithMany()
            .HasForeignKey(x => x.FavorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}