using Algowars.Domain.ExecutionPipelines;
using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Problems.Entities;
using Algowars.Domain.TestSuites.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Algowars.Infrastructure.Persistence.Configuration;

internal sealed class ProblemSetupConfiguration : IEntityTypeConfiguration<ProblemSetup>
{
    public void Configure(EntityTypeBuilder<ProblemSetup> builder)
    {
        builder.ToTable("problem_setups");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property<Guid>("problem_id")
            .HasColumnName("problem_id")
            .IsRequired();

        builder.Property(s => s.LanguageVersionId)
            .HasColumnName("language_version_id")
            .IsRequired();

        builder.HasOne<LanguageVersionEntry>()
            .WithMany()
            .HasForeignKey(s => s.LanguageVersionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.PipelineId)
            .HasColumnName("pipeline_id")
            .IsRequired(true);

        builder.HasOne<ExecutionPipeline>()
            .WithMany()
            .HasForeignKey(s => s.PipelineId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.InitialCode)
            .HasColumnName("initial_code")
            .IsRequired();

        builder.Property(s => s.FunctionName)
            .HasColumnName("function_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasMany(s => s.TestSuites)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "problem_setup_test_suites",
                r => r.HasOne<TestSuite>()
                       .WithMany()
                       .HasForeignKey("test_suite_id")
                       .OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<ProblemSetup>()
                       .WithMany()
                       .HasForeignKey("problem_setup_id")
                       .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("problem_setup_id", "test_suite_id");
                    j.ToTable("problem_setup_test_suites");
                });

        builder.Navigation(s => s.TestSuites)
            .HasField("_testSuites")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}