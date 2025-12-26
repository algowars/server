using ApplicationCore.Dtos.Accounts;

namespace ApplicationCore.Queries.Accounts.GetProfileAggregate;

public record GetProfileAggregateQuery(string Username) : IQuery<ProfileAggregateDto>;
