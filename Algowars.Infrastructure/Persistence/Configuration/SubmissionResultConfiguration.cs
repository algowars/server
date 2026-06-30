using Algowars.Domain.Submissions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Algowars.Infrastructure.Persistence.Configuration;

internal sealed class SubmissionResultConfiguration : IEntityTypeConfiguration<SubmissionResult>
{
    public void Configure(EntityTypeBuilder<SubmissionResult> builder)
    {
        builder.ToTable("submission_results");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property<Guid>("submission_id")
            .HasColumnName("submission_id")
            .IsRequired();

        builder.Property(r => r.TestCaseId)
            .HasColumnName("test_case_id")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.Runtime)
            .HasColumnName("runtime");

        builder.Property(r => r.MemoryUsed)
            .HasColumnName("memory_used");

        builder.Property(r => r.ActualOutput)
            .HasColumnName("actual_output");

        builder.Property(r => r.StandardOutput)
            .HasColumnName("standard_output");

        builder.Property(r => r.StandardError)
            .HasColumnName("standard_error");

        builder.Property(r => r.CompileOutput)
            .HasColumnName("compile_output");

        builder.Property<string?>("execution_id")
            .HasColumnName("execution_id")
            .HasMaxLength(100);

        builder.Property<string?>("evaluation_id")
            .HasColumnName("evaluation_id")
            .HasMaxLength(100);
    }
}
