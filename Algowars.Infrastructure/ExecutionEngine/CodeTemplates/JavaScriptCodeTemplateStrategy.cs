using Algowars.Application.ExecutionEngine;
using System.Text.Json;

namespace Algowars.Infrastructure.ExecutionEngine.CodeTemplates;

internal sealed class JavaScriptCodeTemplateStrategy : ICodeTemplateStrategy
{
    public string LanguageName => "JavaScript";

    public string Render(CodeTemplateContext context)
    {
        return $$"""
            {{context.UserCode}}

            process.stdin.on("data", data => {
                const parsed = JSON.parse(data.toString().trim());
                const args = Array.isArray(parsed) ? parsed : [parsed];
                const result = {{context.FunctionName}}(...args);
                process.stdout.write(String(result).trim());
            });
            """;
    }

    public string BuildStdin(IReadOnlyList<CodeTemplateInput> inputs)
    {
        var values = inputs.Select(i => JsonSerializer.Deserialize<JsonElement>(i.Value)).ToArray();
        return values.Length == 1
            ? JsonSerializer.Serialize(values[0])
            : JsonSerializer.Serialize(values);
    }
}
