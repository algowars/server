using Algowars.Application.Queries;
using Algowars.Application.Users.Dtos;

namespace Algowars.Application.Queries.Users.GetUserBySub;

public sealed record GetUserBySubQuery(string Sub) : IQuery<UserDto>;