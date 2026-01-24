# Copilot Instructions

## General Guidelines
- Use named parameters (e.g., `cancellationToken:`) for `ExecuteUpdateAsync`, `BatchUpdateAsync`, and other EF Core update calls to avoid positional overload resolution issues.

## Code Style
- Implement `IncrementOutboxesCount` when fixing `SubmissionRepository.cs`.