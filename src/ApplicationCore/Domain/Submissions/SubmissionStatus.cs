namespace ApplicationCore.Domain.Submissions;

public enum SubmissionStatus
{
    Accepted = 1,
    WrongAnswer = 2,

    InQueue = 3,
    Processing = 4,
    TimeLimitExceeded = 5,
    CompilationError = 6,

    RuntimeErrorSigSegv = 7,
    RuntimeErrorSigXfsz = 8,
    RuntimeErrorSigFpe = 9,
    RuntimeErrorSigAbrt = 10,
    RuntimeErrorNzec = 11,
    RuntimeErrorOther = 12,

    InternalError = 13,
    ExecFormatError = 14,
}
