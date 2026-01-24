# Copilot Instructions

## General Guidelines
- Use named parameters (e.g., `cancellationToken:`) for `ExecuteUpdateAsync`, `BatchUpdateAsync`, and other EF Core update calls to avoid positional overload resolution issues.

## Code Style
- Implement `IncrementOutboxesCount` when fixing `SubmissionRepository.cs`.
- No comments.

## Project-Specific Rules
- Modify `SubmissionModel.GetOverallStatus` to return a simplified aggregate: 
  - Return `SubmissionStatus.Processing` when there are no results or any result has status `InQueue` or `Processing`. 
  - Return `SubmissionStatus.Accepted` only when all results are `Accepted`. 
  - Otherwise, return `SubmissionStatus.WrongAnswer`.