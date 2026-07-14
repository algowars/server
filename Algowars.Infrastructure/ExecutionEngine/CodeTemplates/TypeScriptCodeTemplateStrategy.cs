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

process.stdin.on("data", data => {
    const parsed = JSON.parse(data.toString().trim());
    const args = Array.isArray(parsed) ? parsed : [parsed];
    const result = FUNCTION_NAME_PLACEHOLDER(...args);
    process.stdout.write(JSON.stringify(result));
});
""";
        return "declare const process: any;\n\n" + context.UserCode + harness.Replace("FUNCTION_NAME_PLACEHOLDER", context.FunctionName);
    }

    /// <summary>Builds the stdin payload — a JSON array of all inputs.</summary>
    public string BuildStdin(IReadOnlyList<CodeTemplateInput> inputs)
        => inputs.Count == 1
            ? inputs[0].Value
            : $"[{string.Join(",", inputs.Select(i => i.Value))}]";
}
