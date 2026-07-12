namespace Algowars.Application.ExecutionEngine;

public interface ICodeTemplateStrategyResolver
{
    ICodeTemplateStrategy Resolve(string languageName);
}
