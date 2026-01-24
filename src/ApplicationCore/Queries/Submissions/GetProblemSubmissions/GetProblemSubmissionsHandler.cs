using System;
using System.Collections.Generic;
using System.Text;
using ApplicationCore.Domain.Submissions;
using Ardalis.Result;

namespace ApplicationCore.Queries.Submissions.GetProblemSubmissions;

public sealed record GetProblemSubmissionsHandler()
    : IQueryHandler<GetProblemSubmissionsQuery, ProblemSubmissions>
{
    public Task<Result<ProblemSubmissions>> Handle(
        GetProblemSubmissionsQuery request,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }
}
