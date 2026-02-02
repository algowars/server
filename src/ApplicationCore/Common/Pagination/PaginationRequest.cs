namespace ApplicationCore.Common.Pagination;

public sealed class PaginationRequest
{
    public required int Page { get; init; }

    public required int Size { get; init; }

    public string? Query { get; init; }

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public SortDirection Direction { get; init; } = SortDirection.Desc;
}