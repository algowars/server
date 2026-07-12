# Submission Pipeline Plan

## Architecture Summary

```
CreateSubmissionHandler
  → saves Submission + SubmissionJob (atomic)
  → domain event fires → publishes queue message (fast-path hint)

Queue Consumer / Quartz fallback
  → calls SubmissionJobProcessor.ProcessAsync()
  → reads SubmissionJob.current_step_id
  → resolves IStepHandler via StepHandlerRegistry
  → handler uses ICodeTemplateStrategy + IExecutionEngineStrategy
  → records SubmissionJobAttempt
  → advances/fails job
```

## New Tables

| Table | Layer |
|---|---|
| execution_pipelines | Domain |
| execution_pipeline_steps | Domain |
| submission_jobs | Domain |
| submission_job_attempts | Domain |
| judge0_step_configurations | Infrastructure |
| assert_step_configurations | Infrastructure |

## Strategy Interfaces (Application layer)

- `IExecutionEngineStrategy` — submit code, poll results
- `ICodeTemplateStrategy` — render user code + inputs into runnable source
- `ICodeTemplateStrategyResolver` — pick the right template by language name
- `IStepHandler` — execute one pipeline step
- `IStepHandlerRegistry` — resolve handler by StepType

## Steps

- [ ] 1.  Add domain enums: ExecutionPipelineStepType, SubmissionJobStatus, SubmissionJobAttemptStatus
- [ ] 2.  Add ExecutionPipeline aggregate + ExecutionPipelineStep entity (Domain)
- [ ] 3.  Add SubmissionJob aggregate + SubmissionJobAttempt entity (Domain)
- [ ] 4.  Add domain repository interfaces: IExecutionPipelineRepository, ISubmissionJobRepository
- [ ] 5.  Add PipelineId to ProblemSetup domain entity
- [ ] 6.  Add Application interfaces: IExecutionEngineStrategy, ICodeTemplateStrategy, ICodeTemplateStrategyResolver, IStepHandler, IStepHandlerRegistry
- [ ] 7.  Add Infrastructure POCOs: Judge0StepConfiguration, AssertStepConfiguration
- [ ] 8.  Add EF configurations for ExecutionPipeline, ExecutionPipelineStep, SubmissionJob, SubmissionJobAttempt
- [ ] 9.  Add EF configurations for Judge0StepConfiguration, AssertStepConfiguration
- [ ] 10. Update ProblemSetupConfiguration for pipeline_id FK
- [ ] 11. Update AlgowarsDbContext with new DbSets
- [ ] 12. Add Judge0Options config class + appsettings.json section
- [ ] 13. Implement Judge0ExecutionEngineStrategy (HttpClient-based)
- [ ] 14. Implement code template strategies: JavaScript, TypeScript, Python
- [ ] 15. Implement CodeTemplateStrategyResolver
- [ ] 16. Implement step handlers: Judge0ExecuteHandler, Judge0PollHandler, EvaluateHandler
- [ ] 17. Implement StepHandlerRegistry
- [ ] 18. Implement SubmissionJobProcessor (core orchestration loop)
- [ ] 19. Implement SubmissionJobProcessorJob (Quartz) + update consumers to call processor
- [ ] 20. Add ExecutionPipelineRepository + SubmissionJobRepository (Infrastructure)
- [ ] 21. Update CreateSubmissionHandler to also create SubmissionJob
- [ ] 22. Register all new services in InfrastructureServiceRegistration
- [ ] 23. Add Judge0 pipeline seeder (well-known GUIDs)
- [ ] 24. Add EF migration
