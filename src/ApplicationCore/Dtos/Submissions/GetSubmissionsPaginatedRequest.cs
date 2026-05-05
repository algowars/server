namespace ApplicationCore.Dtos.Submissions;

public sealed class GetSubmissionsPaginatedRequest
{
    public required Guid ProblemId { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 25;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid? FilterByUserId { get; init; }
    public bool AcceptedOnly { get; init; } = true;
}