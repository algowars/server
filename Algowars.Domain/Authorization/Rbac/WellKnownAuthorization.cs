namespace Algowars.Domain.Authorization.Rbac;

public static class WellKnownAuthorization
{
    public const string CreateSubmissionPermission = "submission:create";
    public const string ReadAdminProblemsPermission = "problem:read:admin";
    public const string ReadAdminUsersPermission = "user:read:admin";

    public const string DefaultUserRole = "default-user";
    public const string DefaultUserGroup = "default-user";

    public const string AdminRole = "admin";
    public const string AdminGroup = "admin";
}
