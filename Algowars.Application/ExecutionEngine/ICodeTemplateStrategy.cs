namespace Algowars.Application.ExecutionEngine;

public sealed record CodeTemplateContext(
    string UserCode,
    string FunctionName,
    IReadOnlyList<CodeTemplateInput> Inputs);

public sealed record CodeTemplateInput(string Value, string ValueType);

public interface ICodeTemplateStrategy
{
    string LanguageName { get; }

    /// <summary>
    /// Renders the complete runnable source file (user code + stdin harness).
    /// The rendered code reads arguments from stdin and prints the result to stdout.
    /// </summary>
    string Render(CodeTemplateContext context);

    /// <summary>
    /// Builds the stdin payload for a single test case — typically one JSON arg per line.
    /// This is passed as the <c>stdin</c> field in the execution engine request.
    /// </summary>
    string BuildStdin(IReadOnlyList<CodeTemplateInput> inputs);
}
