using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;

namespace ApplicationCore.Services;

public sealed class CodeBuilderService : ICodeBuilderService
{
    public Result<IEnumerable<CodeBuildResult>> Build(IEnumerable<CodeBuilderContext> contexts)
    {
        var results = new List<CodeBuildResult>();

        foreach (var context in contexts)
        {
            var validation = ValidateContext(context);
            if (validation != null)
            {
                return validation;
            }

            string finalCode = RenderTemplate(
                context.Template,
                context.InitialCode,
                context.FunctionName,
                context.InputTypeName
            );

            results.Add(
                new CodeBuildResult
                {
                    FinalCode = finalCode,
                    FunctionName = context.FunctionName,
                    Inputs = context.Inputs,
                    ExpectedOutputs = context.ExpectedOutput,
                    InputTypeName = context.InputTypeName,
                }
            );
        }

        return Result<IEnumerable<CodeBuildResult>>.Success(results);
    }

    private static Result<IEnumerable<CodeBuildResult>>? ValidateContext(
        CodeBuilderContext? context
    )
    {
        if (context == null)
        {
            return Result<IEnumerable<CodeBuildResult>>.Invalid(
                new ValidationError("One of the contexts is null.")
            );
        }

        if (string.IsNullOrWhiteSpace(context.InitialCode))
        {
            return Result<IEnumerable<CodeBuildResult>>.Invalid(
                new ValidationError("Initial code is required.")
            );
        }

        if (string.IsNullOrWhiteSpace(context.FunctionName))
        {
            return Result<IEnumerable<CodeBuildResult>>.Invalid(
                new ValidationError("Function name is required.")
            );
        }

        if (string.IsNullOrWhiteSpace(context.Template))
        {
            return Result<IEnumerable<CodeBuildResult>>.Invalid(
                new ValidationError("Harness template is required.")
            );
        }

        return null;
    }

    private static string RenderTemplate(
        string template,
        string userCode,
        string functionName,
        string inputTypeName
    )
    {
        string inputParser = ResolveInputParser(inputTypeName);

        return template
            .Replace("{{USER_CODE}}", userCode)
            .Replace("{{FUNCTION_NAME}}", functionName)
            .Replace("{{INPUT_PARSER}}", inputParser);
    }

    private static string ResolveInputParser(string inputTypeName)
    {
        return inputTypeName switch
        {
            "number" => "const value = parseInt(data.toString(), 10);",
            "array:number" => "const value = data.toString().split(',').map(Number);",
            "string" => "const value = data.toString().trim();",
            _ => "const value = data.toString();",
        };
    }
}
