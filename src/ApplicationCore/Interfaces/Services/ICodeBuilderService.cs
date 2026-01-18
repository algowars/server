using ApplicationCore.Domain.CodeExecution;
using Ardalis.Result;

namespace ApplicationCore.Interfaces.Services;

public interface ICodeBuilderService
{
    Task<Result<IEnumerable<CodeBuildResult>>> BuildAsync(IEnumerable<CodeBuilderContext> contexts);
}
