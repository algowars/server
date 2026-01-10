using System;
using System.Collections.Generic;
using System.Text;
using ApplicationCore.Domain.Problems.ProblemSetups;

namespace ApplicationCore.Domain.CodeExecution;

public sealed class CodeExecutionContext
{
    public required ProblemSetupModel Setup { get; set; }

    public required string Code { get; set; }

    public required IEnumerable<CodeBuildResult> BuiltResults { get; set; }
}
