using Algowars.Application.ExecutionEngine;
using System.Text.Json;

namespace Algowars.Infrastructure.ExecutionEngine.CodeTemplates;

internal sealed class PythonCodeTemplateStrategy : ICodeTemplateStrategy
{
    public string LanguageName => "Python";

    public string Render(CodeTemplateContext context)
    {
        return $$"""
            {{context.UserCode}}

            import sys
            import json

            data = sys.stdin.read()
            args = json.loads(data)
            if not isinstance(args, list):
                args = [args]
            result = {{context.FunctionName}}(*args)
            print(result)
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
