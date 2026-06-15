# 02-domain-layer: Create Algowars.Domain

Build out the pure DDD domain layer. This project has no external NuGet dependencies — only C# and the .NET BCL.

**SeedWork** (from reference branch): `Entity`, `AggregateRoot`, `DomainException`, `IAggregateFactory`.

**Users aggregate** (merges reference branch `User` with current `AccountModel`):
- Entity: `User` — properties: `Id`, `Sub`, `Username`, `Bio`, `ImageUrl`, `UsernameLastChangedAt`; domain methods: `ChangeUsername`, `UpdateBio`, `UpdateImageUrl`; enforces 30-day username cooldown
- Value objects: `Username`, `Bio`, `ImageUrl`
- Exceptions: `InvalidUsernameException`, `InvalidUserSubException`, `InvalidBioException`, `InvalidImageUrlException`, `UsernameCooldownException`
- Repository interface: `IUserRepository`

**Problems aggregate** (from reference branch):
- Entities: `Problem`, `ProblemVersion`, `CodeTemplate`, `Example`, `TestCase`
- Enums: `DifficultyTier`, `ProblemStatus`
- Value objects: `Title`, `Question`, `Slug`, `Difficulty`, `MemoryLimit`, `TimeLimit`
- Exceptions: `InvalidTitleException`, `InvalidQuestionException`, `InvalidSlugException`, `InvalidDifficultyException`, `InvalidMemoryLimitException`, `InvalidTimeLimitException`, `ProblemVersionImmutableException`, `ProblemVersionNotFoundException`
- Repository interface: `IProblemRepository`

**Submissions aggregate** (from reference branch):
- Entities: `Submission`, `SubmissionResult`
- Enums: `SubmissionStatus`, `SubmissionType`, `SubmissionResultStatus`
- Value objects: `SourceCode`
- Exceptions: `InvalidSourceCodeException`, `InvalidSubmissionStateException`, `SubmissionResultNotFoundException`, `SubmissionNotCompleteException`
- Repository interface: `ISubmissionRepository`

**Languages aggregate** (from reference branch):
- Value objects: `LanguageName`, `LanguageSlug`, `LanguageVersion`

**Done when**: `Algowars.Domain` builds without errors or warnings; all entities, value objects, exceptions, and repository interfaces are present; no external NuGet package references exist in the project file.
