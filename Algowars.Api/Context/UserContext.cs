using Algowars.Application.Dtos.Users;

namespace Algowars.Api.Context;

public interface IUserContext
{
    UserDto? User { get; set; }
}

public sealed class UserContext : IUserContext
{
    public UserDto? User { get; set; }
}
