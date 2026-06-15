# Task 02 — Progress Details

## What Changed

Built out the complete `Algowars.Domain` project — pure C#, zero NuGet packages.

### SeedWork
- `Entity` — base class with `Id`, equality by identity, `==`/`!=` operators
- `AggregateRoot` — extends `Entity`, marks aggregate roots
- `DomainException` — abstract base for all domain exceptions
- `IAggregateFactory<TAggregate, TParams>` — factory interface for aggregate creation

### Users aggregate (Account → User rename applied)
- **Entity**: `User` — `Sub`, `Username`, `Bio?`, `ImageUrl?`, `UsernameLastChangedAt`; domain methods: `ChangeUsername` (30-day cooldown), `UpdateBio`, `UpdateImageUrl`
- **Value objects**: `Username` (1–20 chars, alphanumeric/underscore/hyphen), `Bio` (≤500 chars), `ImageUrl` (absolute HTTP/HTTPS, ≤2048 chars)
- **Exceptions**: `InvalidUsernameException`, `InvalidBioException`, `InvalidImageUrlException`, `InvalidUserSubException`, `UsernameCooldownException`
- **Factory**: `UserFactory` / `CreateUserParams`
- **Repository interface**: `IUserRepository`

### Problems aggregate
- **Entities**: `Problem` (aggregate root), `ProblemVersion`, `CodeTemplate`, `Example`, `TestCase`
- **Enums**: `DifficultyTier` (Easy/Medium/Hard), `ProblemStatus` (Draft/Published/Archived)
- **Value objects**: `Title`, `Question`, `Slug` (with `FromTitle` factory), `Difficulty` (with tier calculation), `MemoryLimit`, `TimeLimit`
- **Exceptions**: `InvalidTitleException`, `InvalidQuestionException`, `InvalidSlugException`, `InvalidDifficultyException`, `InvalidMemoryLimitException`, `InvalidTimeLimitException`, `ProblemVersionImmutableException`, `ProblemVersionNotFoundException`
- **Repository interface**: `IProblemRepository`

### Submissions aggregate
- **Entities**: `Submission` (aggregate root), `SubmissionResult`
- **Enums**: `SubmissionStatus`, `SubmissionType`, `SubmissionResultStatus`
- **Value object**: `SourceCode` (≤65536 chars)
- **Exceptions**: `InvalidSourceCodeException`, `InvalidSubmissionStateException`, `SubmissionResultNotFoundException`, `SubmissionNotCompleteException`
- **Repository interface**: `ISubmissionRepository`

### Languages aggregate
- **Entities**: `Language` (aggregate root), `LanguageVersionEntry`
- **Enums**: `LanguageStatus`, `LanguageVersionStatus`
- **Value objects**: `LanguageName`, `LanguageSlug` (with `FromName` factory), `LanguageVersion`
- **Exceptions**: `InvalidLanguageNameException`, `InvalidLanguageSlugException`, `InvalidLanguageVersionException`, `LanguageVersionNotFoundException`
- **Repository interface**: `ILanguageRepository`

## Build Result
`dotnet build Algowars.Domain` — **succeeded** with 0 errors, 0 warnings.

## Commit
`task 02: build out Algowars.Domain with SeedWork, Users, Problems, Submissions, Languages aggregates`
