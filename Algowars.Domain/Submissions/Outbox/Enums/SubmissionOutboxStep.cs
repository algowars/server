namespace Algowars.Domain.Submissions.Outbox.Enums;

public enum SubmissionOutboxStep
{
    Execute = 1,
    PollExecution = 2,
    Evaluate = 3,
    EvaluationPoll = 4
}
