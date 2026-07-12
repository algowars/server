using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Problems.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Algowars.Infrastructure.Persistence.Configuration;

internal sealed class ProblemConfiguration : IEntityTypeConfiguration<Problem>
{
    public void Configure(EntityTypeBuilder<Problem> builder)
    {
        builder.ToTable("problems");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.OwnsOne(p => p.Slug, slug =>
        {
            slug.Property(s => s.Value)
                .HasColumnName("slug")
                .HasMaxLength(Slug.MaxLength)
                .IsRequired();
        });

        builder.Property(p => p.Title)
            .HasColumnName("title")
            .HasMaxLength(Title.MaxLength)
            .IsRequired()
            .HasConversion(
                t => t.Value,
                v => new Title(v));

        builder.Property(p => p.Question)
            .HasColumnName("question")
            .HasMaxLength(Question.MaxLength)
            .IsRequired()
            .HasConversion(
                q => q.Value,
                v => new Question(v));

        builder.Property(p => p.Difficulty)
            .HasColumnName("difficulty")
            .IsRequired()
            .HasConversion(
                d => d.Value,
                v => new Difficulty(v));

        builder.Property(p => p.TimeLimit)
            .HasColumnName("time_limit")
            .IsRequired()
            .HasConversion(
                t => t.Milliseconds,
                v => new TimeLimit(v));

        builder.Property(p => p.MemoryLimit)
            .HasColumnName("memory_limit")
            .IsRequired()
            .HasConversion(
                m => m.Megabytes,
                v => new MemoryLimit(v));

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasMany(p => p.History)
            .WithOne()
            .HasForeignKey("problem_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.History)
            .HasField("_history")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Setups)
            .WithOne()
            .HasForeignKey("problem_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Setups)
            .HasField("_setups")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Tags)
            .WithMany(t => t.Problems)
            .UsingEntity(j => j.ToTable("problem_tags"));

        builder.Navigation(p => p.Tags)
            .HasField("_tags")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}