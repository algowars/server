using Algowars.Application.Dtos.Users;

namespace Algowars.Application.Queries.Users.GetUserBySub;

internal sealed record GetUserBySubQuery(string Sub) : IQuery<UserDto>;
