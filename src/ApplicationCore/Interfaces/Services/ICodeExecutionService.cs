using System;
using System.Collections.Generic;
using System.Text;
using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Domain.Submissions;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface ICodeExecutionService
{
    public Task<Result<SubmissionModel>> ExecuteAsync(
        CodeExecutionContext context,
        CancellationToken cancellationToken
    );
}
