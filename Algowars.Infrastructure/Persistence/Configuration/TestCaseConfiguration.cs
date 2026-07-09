using Algowars.Domain.TestSuites.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Algowars.Infrastructure.Persistence.Configuration;

internal sealed class TestCaseConfiguration : IEntityTypeConfiguration<TestCase>
{
    public void Configure(EntityTypeBuilder<TestCase> builder)
    {
        builder.ToTable("test_cases");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property<Guid>("test_suite_id")
            .HasColumnName("test_suite_id")
            .IsRequired();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .IsRequired(false);

        builder.HasMany(t => t.Inputs)
            .WithOne()
            .HasForeignKey("test_case_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Inputs)
            .HasField("_inputs")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(t => t.ExpectedOutputs)
            .WithOne()
            .HasForeignKey("test_case_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.ExpectedOutputs)
            .HasField("_expectedOutputs")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}