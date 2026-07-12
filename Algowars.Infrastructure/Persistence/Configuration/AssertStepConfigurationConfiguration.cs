using Algowars.Infrastructure.ExecutionEngine.Assert;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Algowars.Infrastructure.Persistence.Configuration;

internal sealed class AssertStepConfigurationConfiguration : IEntityTypeConfiguration<AssertStepConfiguration>
{
    public void Configure(EntityTypeBuilder<AssertStepConfiguration> builder)
    {
        builder.ToTable("assert_step_configurations");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.PipelineStepId)
            .HasColumnName("pipeline_step_id")
            .IsRequired();

        builder.HasIndex(c => c.PipelineStepId).IsUnique();

        builder.Property(c => c.Strategy)
            .HasColumnName("strategy")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.Tolerance).HasColumnName("tolerance");
        builder.Property(c => c.CaseSensitive).HasColumnName("case_sensitive").IsRequired();
    }
}
