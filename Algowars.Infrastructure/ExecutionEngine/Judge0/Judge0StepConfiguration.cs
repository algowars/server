namespace Algowars.Infrastructure.ExecutionEngine.Judge0;

/// <summary>Infrastructure-only POCO — not a domain entity. Mapped by EF for admin configuration.</summary>
public sealed class Judge0StepConfiguration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PipelineStepId { get; set; }
    public bool IsEncoded { get; set; } = true;
    public bool ShouldWait { get; set; } = false;
    public bool StripWhitespace { get; set; } = true;
    public int DefaultTimeoutSeconds { get; set; } = 10;
}
