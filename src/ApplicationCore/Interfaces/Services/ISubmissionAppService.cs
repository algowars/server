namespace ApplicationCore.Interfaces.Services;

public interface ISubmissionAppService
{
    Task<Guid> ExecuteAsync(
        int problemSetupId,
        string code,
        Guid createdById,
        CancellationToken cancellationToken
    );
}
