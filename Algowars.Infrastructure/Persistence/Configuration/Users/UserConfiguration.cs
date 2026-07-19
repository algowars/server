using Algowars.Domain.Authorization.Rbac.Entities;
using Algowars.Domain.Authorization.Rbac.ValueObjects;
using Algowars.Domain.Users.Entities;
using Algowars.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Algowars.Infrastructure.Persistence.Configuration.Users;

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

        builder.HasMany<Group>()
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "user_groups",
                right => right.HasOne<Group>()
                    .WithMany()
                    .HasForeignKey("group_id")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left.HasOne<User>()
                    .WithMany()
                    .HasForeignKey("user_id")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("user_groups");

                    join.Property<Guid>("user_id")
                        .HasColumnName("user_id");

                    join.Property<GroupId>("group_id")
                        .HasColumnName("group_id")
                        .HasConversion(g => g.Value, v => new GroupId(v));

                    join.HasKey("user_id", "group_id");
                    join.HasIndex("group_id")
                        .HasDatabaseName("IX_user_groups_group_id");
                });
    }
}