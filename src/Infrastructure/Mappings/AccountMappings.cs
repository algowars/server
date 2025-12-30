using ApplicationCore.Domain.Accounts;
using Infrastructure.Persistence.Entities.Account;
using Mapster;

namespace Infrastructure.Mappings;

public sealed class AccountMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AccountModel, AccountEntity>().Map(dest => dest.Id, src => src.Id);

        config.NewConfig<AccountEntity, AccountModel>();
    }
}
