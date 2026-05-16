using ApplicationCore.Domain.Accounts;
using Infrastructure.Persistence.Entities.Account;
using Mapster;

namespace Infrastructure.Mappings;

public sealed class AccountMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AccountModel, AccountEntity>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.PreviousUsername, src => src.PreviousUsername)
            .Map(dest => dest.UsernameLastChangedAt, src => src.UsernameLastChangedAt)
            .Map(dest => dest.About, src => src.About);

        config.NewConfig<AccountEntity, AccountModel>()
            .Map(dest => dest.PreviousUsername, src => src.PreviousUsername)
            .Map(dest => dest.UsernameLastChangedAt, src => src.UsernameLastChangedAt)
            .Map(dest => dest.About, src => src.About);
    }
}