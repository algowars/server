namespace ApplicationCore.Commands.Problem.CreateProblem;

public sealed record CreateProblemCommand(
    string Title,
    string Question,
    int EstimatedDifficulty,
    IEnumerable<string> Tags,
    Guid CreatedById
) : ICommand<Guid>;