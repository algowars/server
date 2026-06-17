using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.Sub)
            .HasColumnName("sub")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(u => u.Sub)
            .IsUnique();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(Username.MaxLength)
            .IsRequired()
            .HasConversion(
                u => u.Value,
                v => new Username(v));

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.Bio)
            .HasColumnName("bio")
            .HasMaxLength(Bio.MaxLength)
            .HasConversion(
                b => b != null ? b.Value : null,
                v => v != null ? new Bio(v) : null);

        builder.Property(u => u.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(ImageUrl.MaxLength)
            .HasConversion(
                i => i != null ? i.Value : null,
                v => v != null ? new ImageUrl(v) : null);

        builder.Property(u => u.UsernameLastChangedAt)
            .HasColumnName("username_last_changed_at");
    }
}
