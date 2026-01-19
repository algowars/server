using ApplicationCore.Domain.CodeExecution;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface ICodeBuilderService
{
    Result<IEnumerable<CodeBuildResult>> Build(IEnumerable<CodeBuilderContext> contexts);
}
