# 09-submission-outbox-redesign: Per-step outbox ledger with retry tracking

Redesign the submission processing outbox from a single mutating row (where `SubmissionOutboxType` is overwritten as the submission advances) into an append-only ledger where **each pipeline step inserts its own row**. This gives a per-step retry count and an independent status for every stage, making it easy to answer "the Execute step needed 3 attempts; Evaluate needed 1".

#### Domain design (`Algowars.Domain/Submissions/Outbox/`)

**`SubmissionOutboxStep`** enum — the five pipeline stages, each maps to one row class:
```
Execute = 1         // Initial code execution request to Judge0
PollExecution = 2   // Polling Judge0 for execution result tokens
Evaluate = 3        // Comparing execution output against expected answers
EvaluationPoll = 4  // Polling final evaluation state before completing
```

**`SubmissionOutboxStatus`** enum — per-row lifecycle:
```
Pending    = 1  // Row created, not yet picked up
Processing = 2  // Worker is actively working on this step
Retrying   = 3  // Previous attempt failed; scheduled for another attempt
Completed  = 4  // Step finished successfully
Failed     = 5  // Step exhausted max retries; pipeline cannot continue
Abandoned  = 6  // Step was superseded or the submission was cancelled
```

**`SubmissionOutbox`** aggregate root (replaces `SubmissionOutboxModel`):
```csharp
public sealed class SubmissionOutbox : AggregateRoot
{
    public Guid SubmissionId       { get; }
    public SubmissionOutboxStep  Step       { get; }
    public SubmissionOutboxStatus Status    { get; private set; }
    public int  AttemptCount       { get; private set; }
    public int  MaxAttempts        { get; }          // default 5
    public DateTime  CreatedAt     { get; }
    public DateTime? LastAttemptAt { get; private set; }
    public DateTime? CompletedAt   { get; private set; }
    public string?   LastError     { get; private set; }

    // Domain methods
    public void RecordAttempt(DateTime now);   // Pending/Retrying → Processing
    public void Complete(DateTime now);        // Processing → Completed
    public void RecordFailure(string error, DateTime now);  // → Retrying or Failed
    public bool CanRetry { get; }             // AttemptCount < MaxAttempts
}
```

**Factory method** — the step transition creates the next row:
```csharp
public static SubmissionOutbox CreateForStep(Guid submissionId, SubmissionOutboxStep step, int maxAttempts = 5);
```

**Repository interface** (`ISubmissionOutboxRepository` — new, extracted from `ISubmissionRepository`):
```csharp
Task<IReadOnlyList<SubmissionOutbox>> GetPendingByStepAsync(SubmissionOutboxStep step, int batchSize, CancellationToken ct);
Task AddAsync(SubmissionOutbox outbox, CancellationToken ct);
Task UpdateAsync(SubmissionOutbox outbox, CancellationToken ct);
Task<IReadOnlyList<SubmissionOutbox>> GetBySubmissionIdAsync(Guid submissionId, CancellationToken ct);
```

#### Infrastructure changes (`Algowars.Infrastructure`)

**`SubmissionOutboxDataModel`** — single table `submission_outbox_steps`, columns: `id`, `submission_id`, `step` (int), `status` (int), `attempt_count`, `max_attempts`, `created_at`, `last_attempt_at`, `completed_at`, `last_error`. Replace the old `SubmissionOutboxEntity`, `SubmissionOutboxStatusEntity`, `SubmissionOutboxTypeEntity` tables — the status and step are now simple ints (no lookup tables needed).

**`SubmissionOutboxRepository`** — implements `ISubmissionOutboxRepository`. Uses `SELECT ... WHERE step = @step AND status IN (Pending, Retrying) AND (last_attempt_at IS NULL OR last_attempt_at < @now - interval)` for polling.

**Existing `ISubmissionRepository`** — remove all outbox methods (`GetSubmissionOutboxesAsync`, `IncrementOutboxesCountAsync`, `FinalizeEvaluationAsync`, etc.). Those concerns move to `ISubmissionOutboxRepository`.

#### Application layer changes (`Algowars.Application`)

Remove `IncrementSubmissionOutboxesCommand` and replace with explicit step-lifecycle commands:
- `BeginOutboxStepCommand(SubmissionOutboxStep step, Guid submissionId)` — inserts a new outbox row with status Pending
- `RecordOutboxAttemptCommand(Guid outboxId)` — transitions Pending/Retrying → Processing
- `CompleteOutboxStepCommand(Guid outboxId)` — transitions Processing → Completed; if there is a next step, dispatches `BeginOutboxStepCommand` for it
- `FailOutboxStepCommand(Guid outboxId, string error)` — transitions Processing → Retrying (if CanRetry) or Failed

Update the Quartz job and MassTransit consumers to call the new commands instead of the raw `IncrementOutboxesCountAsync` / `FinalizeEvaluationAsync` calls.

Add a `GetOutboxByStepQuery(SubmissionOutboxStep step)` that replaces `GetSubmissionOutboxesQuery` — returns only rows for the requested step.

#### EF migration

After implementing the above, add a new EF migration `AddSubmissionOutboxSteps` that creates the `submission_outbox_steps` table and drops (or renames) the three old outbox tables (`submission_outbox`, `submission_outbox_statuses`, `submission_outbox_types`).

**Done when**: The `submission_outbox_steps` table is the sole outbox persistence store; each pipeline step (Execute, PollExecution, Evaluate, EvaluationPoll) inserts a new row on entry; `AttemptCount` increments on each retry without creating a new row; job and consumer tests confirm per-step status tracking; full solution builds with zero errors and zero warnings; all unit tests pass.
