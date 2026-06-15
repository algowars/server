using Algowars.Domain.Submissions;
using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.Submissions.Enums;
using Algowars.Domain.Submissions.ValueObjects;
using Algowars.Infrastructure.Persistence;
using Algowars.Infrastructure.Persistence.Entities.Submissions;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Repositories;

internal sealed class SubmissionRepository(AlgoWarsDbContext db) : ISubmissionRepository
{
    public async Task AddAsync(Submission submission, CancellationToken cancellationToken)
    {
        var model = new SubmissionDataModel
        {
            Id = submission.Id,
            UserId = submission.UserId,
            ProblemVersionId = submission.ProblemVersionId,
            LanguageVersionId = submission.LanguageVersionId,
            Type = (int)submission.Type,
            SourceCode = submission.SourceCode.Value,
            Status = (int)submission.Status,
            CreatedOn = submission.CreatedOn,
            TestCases = submission.Results.Select(r => new SubmissionTestCaseDataModel
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                TestCaseId = r.TestCaseId,
            }).ToList(),
        };

        db.Submissions.Add(model);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Submission?> FindByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var model = await db.Submissions
            .Include(s => s.Results)
            .Include(s => s.TestCases)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        return model is null ? null : MapToDomain(model);
    }

    public async Task UpdateAsync(Submission submission, CancellationToken cancellationToken)
    {
        var model = await db.Submissions
            .Include(s => s.Results)
            .FirstOrDefaultAsync(s => s.Id == submission.Id, cancellationToken);

        if (model is null) return;

        model.Status = (int)submission.Status;

        foreach (var result in submission.Results)
        {
            var existing = model.Results.FirstOrDefault(r => r.TestCaseId == result.TestCaseId);
            if (existing is not null)
            {
                existing.Status = (int)result.Status;
                existing.RuntimeMs = result.Runtime;
                existing.MemoryKb = result.MemoryUsed;
                existing.ActualOutput = result.ActualOutput;
                existing.Stderr = result.StandardError;
                existing.CompileOutput = result.CompileOutput;
            }
            else
            {
                model.Results.Add(new SubmissionResultDataModel
                {
                    Id = result.Id,
                    SubmissionId = submission.Id,
                    TestCaseId = result.TestCaseId,
                    Status = (int)result.Status,
                    RuntimeMs = result.Runtime,
                    MemoryKb = result.MemoryUsed,
                    ActualOutput = result.ActualOutput,
                    Stderr = result.StandardError,
                    CompileOutput = result.CompileOutput,
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Submission MapToDomain(SubmissionDataModel model)
    {
        var testCaseIds = model.TestCases.Select(tc => tc.TestCaseId);
        var submission = new Submission(
            model.UserId,
            model.ProblemVersionId,
            model.LanguageVersionId,
            (SubmissionType)model.Type,
            new SourceCode(model.SourceCode),
            testCaseIds);
        return submission;
    }
}
