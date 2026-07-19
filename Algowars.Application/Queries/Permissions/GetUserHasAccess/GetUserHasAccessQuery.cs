using Algowars.Application.Queries;

namespace Algowars.Application.Queries.Permissions.GetUserHasAccess;

public sealed record GetUserHasAccessQuery(Guid UserId, string PermissionCode) : IQuery<bool>;