using Algowars.Application.ExecutionEngine;
using System.Text.Json;

namespace Algowars.Infrastructure.ExecutionEngine.CodeTemplates;

internal sealed class TypeScriptCodeTemplateStrategy : ICodeTemplateStrategy
{
    public string LanguageName => "TypeScript";

    public string Render(CodeTemplateContext context)
    {
        return $$"""
        declare const process: {
            stdin: { on(event: "data", listener: (data: any) => void): void };
            stdout: { write(chunk: string): void };
        };

        {{context.UserCode}}

        process.stdin.on("data", (data: any) => {
            const parsed = JSON.parse(data.toString().trim());
            const args: any[] = Array.isArray(parsed) ? parsed : [parsed];
            const result = ({{context.FunctionName}} as any)(...args);
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
