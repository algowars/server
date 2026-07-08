using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.Submissions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Algowars.Infrastructure.Persistence.Configuration;

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
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(s => s.SourceCode)
            .HasConversion(
                v => v.Value,
                v => new SourceCode(v))
            .HasColumnName("source_code")
            .HasMaxLength(SourceCode.MaxLength)
            .IsRequired();

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