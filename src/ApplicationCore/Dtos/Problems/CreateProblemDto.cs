namespace ApplicationCore.Dtos.Problems;

public sealed record CreateProblemDto(
    string Title,
    int EstimatedDifficulty,
    string Question,
    IEnumerable<string> Tags
);