using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Submissions.Events;

public sealed record SubmissionCreatedDomainEvent(Guid SubmissionId) : IDomainEvent;
