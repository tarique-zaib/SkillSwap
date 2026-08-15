using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AddressLine1)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.AddressLine2)
            .HasMaxLength(250);

        builder.Property(x => x.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.State)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.PostalCode);
    }
}