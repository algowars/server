using Algowars.Domain.Problems.Enums;

namespace Algowars.Application.Problems.Dtos;

public sealed record ProblemDto(Guid Id, string Slug, string Title, DifficultyTier DifficultyTier, IReadOnlyList<string> Tags);