using Algowars.Domain.SeedWork;

namespace Algowars.Domain.Problem.Entities;

public sealed class CodeTemplate : Entity
{
    private CodeTemplate() { }

    internal CodeTemplate(Guid languageId, string starterCode, string wrapperCode)
    {
        LanguageId = languageId;
        StarterCode = starterCode ?? throw new ArgumentNullException(nameof(starterCode));
        WrapperCode = wrapperCode ?? throw new ArgumentNullException(nameof(wrapperCode));
    }

    public Guid LanguageId { get; private set; }
    public string StarterCode { get; private set; } = string.Empty;
    public string WrapperCode { get; private set; } = string.Empty;
}
