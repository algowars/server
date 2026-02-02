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
    }

    public static class Exceptions
    {
        public const int UnhandledException = 1000;
        public const int UnhandledExceptionWithPath = 1001;
    }
}