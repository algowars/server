using Algowars.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Algowars.Infrastructure.Persistence.Configuration;

internal sealed class SeedHistoryConfiguration : IEntityTypeConfiguration<SeedHistoryEntry>
{
    public void Configure(EntityTypeBuilder<SeedHistoryEntry> builder)
    {
        builder.ToTable("seed_history");

        builder.HasKey(e => e.Name);

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(e => e.ExecutedAt)
            .HasColumnName("executed_at")
            .IsRequired();
    }
}
