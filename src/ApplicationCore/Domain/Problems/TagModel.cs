namespace ApplicationCore.Domain.Problems;

public sealed class Tag
{
    public int Id { get; init; }
    
    public required string Value { get; init; }
}