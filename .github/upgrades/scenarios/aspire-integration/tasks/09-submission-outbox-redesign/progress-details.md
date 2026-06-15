## Files Modified

### Domain (`Algowars.Domain`)
- `Submissions/Outbox/Enums/SubmissionOutboxStep.cs` — new enum: Execute, PollExecution, Evaluate, EvaluationPoll
- `Submissions/Outbox/Enums/SubmissionOutboxStatus.cs` — new enum: Pending, Processing, Retrying, Completed, Failed, Abandoned
- `Submissions/Outbox/SubmissionOutbox.cs` — new aggregate root with CreateForStep, Reconstitute, RecordAttempt, Complete, RecordFailure, CanRetry
- `Submissions/Outbox/ISubmissionOutboxRepository.cs` — new repository interface

### Infrastructure (`Algowars.Infrastructure`)
- `Persistence/Entities/Submissions/SubmissionOutboxDataModel.cs` — new data model mapping to `submission_outbox_steps`
- `Persistence/AlgoWarsDbContext.cs` — added `DbSet<SubmissionOutboxDataModel> SubmissionOutboxSteps`
- `Repositories/SubmissionOutboxRepository.cs` — new repository; uses `Reconstitute` factory for domain mapping
- `InfrastructureServiceRegistration.cs` — registered `ISubmissionOutboxRepository`
- `Messaging/Consumers/SubmissionCreatedConsumer.cs` — updated log message
- `Persistence/Migrations/20260615023526_AddSubmissionOutboxSteps.cs` — EF migration creating `submission_outbox_steps`
- `Persistence/Migrations/20260615023526_AddSubmissionOutboxSteps.Designer.cs` — EF designer file

### Application (`Algowars.Application`)
- `Commands/Submissions/Outbox/BeginOutboxStepCommand.cs` + `Handler` — inserts new outbox row for a pipeline step
- `Commands/Submissions/Outbox/RecordOutboxAttemptCommand.cs` + `Handler` — Pending/Retrying → Processing
- `Commands/Submissions/Outbox/CompleteOutboxStepCommand.cs` + `Handler` — Processing → Completed; auto-seeds next step via BeginOutboxStepCommand
- `Commands/Submissions/Outbox/FailOutboxStepCommand.cs` + `Handler` — → Retrying (if CanRetry) or Failed
- `Queries/Submissions/GetOutboxByStep/GetOutboxByStepQuery.cs` + `Handler` — returns pending rows for a given step
- `Dtos/Submissions/SubmissionOutboxDto.cs` — new DTO
- `Commands/Submissions/CreateSubmission/CreateSubmissionHandler.cs` — now seeds Execute outbox row after persisting submission

## Build Results

Full solution (`Algowars.slnx`): 0 errors; MSB3277 warnings only in `Algowars.UnitTests` (EFCore.Relational 10.0.8 vs 10.0.9 version conflict — deferred to Task 07). All production projects clean.

## Pipeline Flow

```
CreateSubmission → BeginOutboxStep(Execute)
  → worker picks up Execute row → RecordOutboxAttempt → ... → CompleteOutboxStep
	→ CompleteOutboxStepHandler auto-seeds BeginOutboxStep(PollExecution)
	  → ... → CompleteOutboxStep
		→ auto-seeds BeginOutboxStep(Evaluate)
		  → ... → CompleteOutboxStep
			→ auto-seeds BeginOutboxStep(EvaluationPoll)
			  → CompleteOutboxStep → no next step → done
```

Each step row: independent AttemptCount, Status, LastError — full per-step retry history.

## Commit

`1f25761` — feat: submission outbox append-only per-step ledger (Task 09)
