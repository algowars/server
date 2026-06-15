using Algowars.Application.Dtos.Users;

namespace Algowars.Application.Queries.Users.GetProfileAggregate;

public sealed record GetProfileAggregateQuery(string Username) : IQuery<ProfileAggregateDto>;
