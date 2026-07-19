using Algowars.Domain.ExecutionPipelines.Enums;

namespace Algowars.Application.ExecutionEngine;

public interface IStepHandlerRegistry
{
    IStepHandler Resolve(ExecutionPipelineStepType stepType);
}
