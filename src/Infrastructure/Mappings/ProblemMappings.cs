using System;
using System.Collections.Generic;
using System.Text;
using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Domain.Problems.TestSuites;
using Infrastructure.Persistence.Entities.Language;
using Infrastructure.Persistence.Entities.Problem;
using Infrastructure.Persistence.Entities.TestSuite;
using Mapster;

namespace Infrastructure.Mappings;

public sealed class ProblemMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ProblemModel, ProblemEntity>();

        config
            .NewConfig<ProblemEntity, ProblemModel>()
            .Ignore(s => s.Status)
            .Map(d => d.Status, s => (ProblemStatus)s.StatusId);

        config
            .NewConfig<ProblemSetupEntity, ProblemSetupModel>()
            .Map(dest => dest.LanguageVersion, src => src.LanguageVersion)
            .Map(dest => dest.TestSuites, src => src.TestSuites);

        config
            .NewConfig<LanguageVersionEntity, LanguageVersion>()
            .Map(dest => dest.ProgrammingLanguageId, src => src.ProgrammingLanguageId);

        config
            .NewConfig<TestSuiteEntity, TestSuiteModel>()
            .Map(dest => dest.TestSuiteType, src => (TestSuiteType)src.TestSuiteTypeId)
            .Map(dest => dest.TestCases, src => src.TestCases);

        config
            .NewConfig<TestCaseEntity, TestCaseModel>()
            .Map(dest => dest.Input, src => src.IoPayload.Input)
            .Map(dest => dest.ExpectedOutput, src => src.IoPayload.ExpectedOutput)
            .Map(dest => dest.TestCaseType, src => (TestCaseType)src.TestCaseTypeId);
    }
}
