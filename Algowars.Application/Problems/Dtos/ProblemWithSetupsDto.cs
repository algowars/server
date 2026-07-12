using Algowars.Domain.Problems.Enums;

namespace Algowars.Application.Problems.Dtos;

public sealed record ProblemWithSetupsDto(Guid Id, string Slug, string Title, DifficultyTier DifficultyTier, string Question, IEnumerable<ProblemSetupLanguageDto> AvailableLanguages, IEnumerable<PublicTestCaseDto> PublicTestCases);