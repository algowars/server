using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;

namespace ApplicationCore.Queries.Accounts.GetProfileAggregate;

public sealed class GetProfileAggregateHandler(IAccountRepository repository)
    : IQueryHandler<GetProfileAggregateQuery, ProfileAggregateDto>
{
    public async Task<Result<ProfileAggregateDto>> Handle(
        GetProfileAggregateQuery request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return Result.Invalid([
                new ValidationError(nameof(request.Username), "Username is required"),
            ]);
        }

        try
        {
            var profile = await repository.GetByUsernameAsync(request.Username, cancellationToken);

            if (profile is null)
            {
                return Result.NotFound();
            }

            var dto = new ProfileAggregateDto(
                new AccountDto
                {
                    Id = profile.Id,
                    Username = profile.Username,
                    ImageUrl = profile.ImageUrl,
                    CreatedOn = profile.CreatedOn,
                }
            );

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}