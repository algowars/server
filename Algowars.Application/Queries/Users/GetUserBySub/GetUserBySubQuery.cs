using Algowars.Application.Dtos.Users;

namespace Algowars.Application.Queries.Users.GetUserBySub;

public sealed record GetUserBySubQuery(string Sub) : IQuery<UserDto>;
