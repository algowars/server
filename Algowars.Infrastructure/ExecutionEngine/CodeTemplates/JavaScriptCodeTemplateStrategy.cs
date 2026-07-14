using Algowars.Application.ExecutionEngine;

namespace Algowars.Infrastructure.ExecutionEngine.CodeTemplates;

/// <summary>
/// Renders a complete Node.js source file.
/// Inputs are passed via stdin (one JSON-serialised argument per line).
/// The harness reads stdin, parses each line as JSON, spreads them as args,
/// then prints the result as JSON to stdout — matching Judge0 examples.
/// </summary>
internal sealed class JavaScriptCodeTemplateStrategy : ICodeTemplateStrategy
{
    public string LanguageName => "javascript";

    public string Render(CodeTemplateContext context)
    {
        const string harness = """


process.stdin.resume();
process.stdin.setEncoding('utf8');
let _input = '';
process.stdin.on('data', d => _input += d);
process.stdin.on('end', () => {
    const _args = _input.trim().split('\n').map(line => JSON.parse(line));
    const _result = FUNCTION_NAME_PLACEHOLDER(..._args);
    console.log(JSON.stringify(_result));
});
""";
        return context.UserCode + harness.Replace("FUNCTION_NAME_PLACEHOLDER", context.FunctionName);
    }

    /// <summary>Builds the stdin payload — a JSON array of all inputs.</summary>
    public string BuildStdin(IReadOnlyList<CodeTemplateInput> inputs)
        => inputs.Count == 1
            ? inputs[0].Value
            : $"[{string.Join(",", inputs.Select(i => i.Value))}]";
}
