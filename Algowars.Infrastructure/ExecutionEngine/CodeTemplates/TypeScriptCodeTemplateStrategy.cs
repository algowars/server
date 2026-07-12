using Algowars.Application.ExecutionEngine;

namespace Algowars.Infrastructure.ExecutionEngine.CodeTemplates;

/// <summary>
/// Renders a complete TypeScript source file for Judge0 (compiled via ts-node or tsc).
/// Inputs are passed via stdin (one JSON-serialised argument per line).
/// </summary>
internal sealed class TypeScriptCodeTemplateStrategy : ICodeTemplateStrategy
{
    public string LanguageName => "typescript";

    public string Render(CodeTemplateContext context)
    {
        const string harness = """


process.stdin.resume();
process.stdin.setEncoding('utf8');
let _input: string = '';
process.stdin.on('data', (d: string) => _input += d);
process.stdin.on('end', () => {
    const _args: unknown[] = _input.trim().split('\n').map((line: string) => JSON.parse(line));
    const _result = FUNCTION_NAME_PLACEHOLDER(...(_args as Parameters<typeof FUNCTION_NAME_PLACEHOLDER>));
    console.log(JSON.stringify(_result));
});
""";
        return context.UserCode + harness.Replace("FUNCTION_NAME_PLACEHOLDER", context.FunctionName);
    }

    /// <summary>Builds the stdin payload — one JSON arg per line.</summary>
    public string BuildStdin(IReadOnlyList<CodeTemplateInput> inputs)
        => string.Join('\n', inputs.Select(i => i.Value));
}
