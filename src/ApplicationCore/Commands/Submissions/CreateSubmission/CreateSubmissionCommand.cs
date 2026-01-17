namespace ApplicationCore.Commands.Submissions.CreateSubmission;

public sealed record CreateSubmissionCommand(int ProblemSetupId, string Code, Guid CreatedById)
    : ICommand<Guid>;
