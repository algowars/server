using ApplicationCore.Domain.Accounts;
using ApplicationCore.Domain.Submissions;
using Infrastructure.Persistence.Entities.Account;
using Infrastructure.Persistence.Entities.Submission;
using Mapster;

namespace Infrastructure.Mappings;

public sealed class SubmissionMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AccountEntity, AccountModel>()
            .Ignore(dest => dest.PreviousUsername)
            .Ignore(dest => dest.UsernameLastChangedAt);

        config.NewConfig<SubmissionEntity, SubmissionModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.ProblemSetupId, src => src.ProblemSetupId)
            .Map(dest => dest.CreatedOn, src => src.CreatedOn)
            .Map(dest => dest.CompletedAt, src => src.CompletedAt)
            .Map(dest => dest.CreatedById, src => src.CreatedById)
            .Map(dest => dest.CreatedBy, src => src.CreatedBy);
    }
}
