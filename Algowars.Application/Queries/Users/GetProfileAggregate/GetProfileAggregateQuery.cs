using Algowars.Application.Dtos.Users;

namespace Algowars.Application.Queries.Users.GetProfileAggregate;

internal sealed record GetProfileAggregateQuery(string Username) : IQuery<ProfileAggregateDto>;
