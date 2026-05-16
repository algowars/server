namespace ApplicationCore.Logging;

public static class LoggingEventIds
{
    public static class Accounts
    {
        public const int UsernameInvalid = 2000;
        public const int DuplicateUsername = 2001;
        public const int DuplicateSub = 2002;
        public const int DuplicateUsernameOrSub = 2003;

        public const int CreateAttempt = 2100;
        public const int Created = 2101;
        public const int CreateDuplicateDetectedPreQuery = 2102;
        public const int CreateDuplicateRace = 2103;
        public const int NotFoundBySub = 2104;
        public const int CreateFailed = 2105;

        public const int UpdateUsernameFailed = 2301;

        public const int ContextMissingSub = 2200;
        public const int ContextResolveFailed = 2201;
    }

    public static class Exceptions
    {
        public const int UnhandledException = 1000;
        public const int UnhandledExceptionWithPath = 1001;
    }

    public static class Jobs
    {
        public const int Started = 3000;
        public const int Completed = 3001;
        public const int Failed = 3002;

        public const int SubmissionExecutionProcessing = 3100;

        public const int PollSubmissionExecutionPolling = 3200;

        public const int EvaluateSubmissionEvaluating = 3300;
        public const int EvaluateSubmissionEvaluated = 3301;

        public const int PollEvaluationFinalizing = 3400;
    }

    public static class Submissions
    {
        public const int Created = 4000;
        public const int CreateFailed = 4001;

        // Stage 1: SubmissionCreatedConsumer
        public const int Stage1Started = 4100;
        public const int Stage1OutboxNotFound = 4101;
        public const int Stage1SetupFailed = 4102;
        public const int Stage1BuildFailed = 4103;
        public const int Stage1ExecutionFailed = 4104;
        public const int Stage1Completed = 4105;

        // Stage 2: SubmissionExecutedConsumer (poll Judge0)
        public const int Stage2Started = 4200;
        public const int Stage2OutboxNotFound = 4201;
        public const int Stage2PollFailed = 4202;
        public const int Stage2StillProcessing = 4203;
        public const int Stage2Completed = 4204;

        // Stage 3: SubmissionReadyToEvaluateConsumer
        public const int Stage3Started = 4300;
        public const int Stage3OutboxNotFound = 4301;
        public const int Stage3SetupNotFound = 4302;
        public const int Stage3Completed = 4303;

        // Stage 4: SubmissionEvaluationPollConsumer
        public const int Stage4Started = 4400;
        public const int Stage4OutboxNotFound = 4401;
        public const int Stage4Completed = 4402;
    }

    public static class Judge0
    {
        public const int SubmitStarted = 5000;
        public const int SubmitCompleted = 5001;
        public const int SubmitFailed = 5002;

        public const int GetStarted = 5100;
        public const int GetCompleted = 5101;
        public const int GetFailed = 5102;
    }
}