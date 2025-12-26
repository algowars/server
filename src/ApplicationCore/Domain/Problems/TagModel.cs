namespace ApplicationCore.Domain.Problems;

public sealed class TagModel
{
    public int Id { get; init; }
    
    public required string Value { get; init; }
}