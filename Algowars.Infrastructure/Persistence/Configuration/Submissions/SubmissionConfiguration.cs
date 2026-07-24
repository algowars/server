using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.Submissions.ValueObjects;
using Algowars.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Algowars.Infrastructure.Persistence.Configuration.Submissions;

internal sealed class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("submissions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(s => s.ProblemSetupId)
            .HasColumnName("problem_setup_id")
            .IsRequired();

        builder.Property(s => s.Type)
            .HasColumnName("type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.SourceCode)
            .HasConversion(
                v => v.Value,
                v => new SourceCode(v))
            .HasColumnName("source_code")
            .HasMaxLength(SourceCode.MaxLength)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProblemSetup>()
            .WithMany()
            .HasForeignKey(s => s.ProblemSetupId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Results)
            .WithOne()
            .HasForeignKey("submission_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Results)
            .HasField("_results")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}