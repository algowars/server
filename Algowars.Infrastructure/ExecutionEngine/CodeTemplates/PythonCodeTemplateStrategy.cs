using Algowars.Application.ExecutionEngine;

namespace Algowars.Infrastructure.ExecutionEngine.CodeTemplates;

/// <summary>
/// Renders a complete Python 3 source file for Judge0.
/// Inputs are passed via stdin (one JSON-serialised argument per line).
/// The harness reads stdin, parses each line with json.loads, unpacks them
/// as positional args, then prints the result as JSON — matching Judge0 examples.
/// </summary>
internal sealed class PythonCodeTemplateStrategy : ICodeTemplateStrategy
{
    public string LanguageName => "python";

    public string Render(CodeTemplateContext context)
    {
        const string harness = """


import sys
import json

def _main():
    lines = sys.stdin.read().strip().split('\n')
    args = [json.loads(line) for line in lines]
    result = FUNCTION_NAME_PLACEHOLDER(*args)
    print(json.dumps(result))

_main()
""";
        return context.UserCode + harness.Replace("FUNCTION_NAME_PLACEHOLDER", context.FunctionName);
    }

    /// <summary>Builds the stdin payload — one JSON arg per line.</summary>
    public string BuildStdin(IReadOnlyList<CodeTemplateInput> inputs)
        => string.Join('\n', inputs.Select(i => i.Value));
}
