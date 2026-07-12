using Algowars.Application.ExecutionEngine;

namespace Algowars.Infrastructure.ExecutionEngine.CodeTemplates;

internal sealed class CodeTemplateStrategyResolver(IEnumerable<ICodeTemplateStrategy> strategies)
    : ICodeTemplateStrategyResolver
{
    private readonly IReadOnlyDictionary<string, ICodeTemplateStrategy> _map =
        strategies.ToDictionary(s => s.LanguageName, StringComparer.OrdinalIgnoreCase);

    public ICodeTemplateStrategy Resolve(string languageName)
        => _map.TryGetValue(languageName, out var strategy)
            ? strategy
            : throw new InvalidOperationException(
                $"No code template strategy registered for language '{languageName}'. " +
                $"Registered: {string.Join(", ", _map.Keys)}");
}
