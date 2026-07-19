using Algowars.Application.Users.Dtos;

namespace Algowars.Application.Queries.Permissions.GetUserAccessContext;

public sealed record GetUserAccessContextQuery(string Sub) : IQuery<UserAccessContextDto>;