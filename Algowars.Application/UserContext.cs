using Algowars.Application.Users.Dtos;

namespace Algowars.Application;

public sealed class UserContext
{
    public UserDto? User { get; set; }

    public IReadOnlyList<string> Permissions { get; set; } = [];

    public IReadOnlyList<string> Roles { get; set; } = [];

    public bool IsAuthenticated => User is not null;

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission);
}