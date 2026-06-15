using Algowars.Domain.Submissions.Outbox;
using Algowars.Domain.Submissions.Outbox.Enums;
using Algowars.Infrastructure.Persistence;
using Algowars.Infrastructure.Persistence.Entities.Submissions;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class SubmissionOutboxRepository(AlgoWarsDbContext db) : ISubmissionOutboxRepository
{
    public async Task AddAsync(SubmissionOutbox outbox, CancellationToken cancellationToken)
    {
        db.SubmissionOutboxSteps.Add(MapToData(outbox));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SubmissionOutbox outbox, CancellationToken cancellationToken)
    {
        var model = await db.SubmissionOutboxSteps
            .FirstOrDefaultAsync(x => x.Id == outbox.Id, cancellationToken);

        if (model is null)
            return;

        model.Status = (int)outbox.Status;
        model.AttemptCount = outbox.AttemptCount;
        model.LastAttemptAt = outbox.LastAttemptAt;
        model.CompletedAt = outbox.CompletedAt;
        model.LastError = outbox.LastError;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<SubmissionOutbox?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await db.SubmissionOutboxSteps
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return model is null ? null : MapToDomain(model);
    }

    public async Task<IReadOnlyList<SubmissionOutbox>> GetPendingByStepAsync(
        SubmissionOutboxStep step,
        int batchSize,
        CancellationToken cancellationToken)
    {
        int stepInt = (int)step;
        int[] eligibleStatuses = [(int)SubmissionOutboxStatus.Pending, (int)SubmissionOutboxStatus.Retrying];

        var models = await db.SubmissionOutboxSteps
            .AsNoTracking()
            .Where(x => x.Step == stepInt && eligibleStatuses.Contains(x.Status))
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        return models.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<SubmissionOutbox>> GetBySubmissionIdAsync(
        Guid submissionId,
        CancellationToken cancellationToken)
    {
        var models = await db.SubmissionOutboxSteps
            .AsNoTracking()
            .Where(x => x.SubmissionId == submissionId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(MapToDomain).ToList();
    }

    private static SubmissionOutboxDataModel MapToData(SubmissionOutbox outbox)
        => new()
        {
            Id = outbox.Id,
            SubmissionId = outbox.SubmissionId,
            Step = (int)outbox.Step,
            Status = (int)outbox.Status,
            AttemptCount = outbox.AttemptCount,
            MaxAttempts = outbox.MaxAttempts,
            CreatedAt = outbox.CreatedAt,
            LastAttemptAt = outbox.LastAttemptAt,
            CompletedAt = outbox.CompletedAt,
            LastError = outbox.LastError,
        };

    private static SubmissionOutbox MapToDomain(SubmissionOutboxDataModel model)
        => SubmissionOutbox.Reconstitute(
            model.Id,
            model.SubmissionId,
            (SubmissionOutboxStep)model.Step,
            (SubmissionOutboxStatus)model.Status,
            model.AttemptCount,
            model.MaxAttempts,
            model.CreatedAt,
            model.LastAttemptAt,
            model.CompletedAt,
            model.LastError);
}
