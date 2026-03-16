using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Services;
using Ardalis.Result;

namespace UnitTests.ApplicationCore.Services;

[TestFixture]
public sealed class CodeBuilderServiceTests
{
    private CodeBuilderService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new CodeBuilderService();
    }

    [Test]
    public void Build_returns_success_with_rendered_code_for_valid_context()
    {
        var contexts = new List<CodeBuilderContext>
        {
            new()
            {
                Code = "function twoSum(nums, target) { return [0, 1]; }",
                Template = "{{USER_CODE}}\nconsole.log({{FUNCTION_NAME}}({{INPUT_PARSER}}));",
                FunctionName = "twoSum",
                Inputs = "1,2",
                ExpectedOutput = "[0,1]",
            },
        };

        var result = _sut.Build(contexts);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Exactly(1).Items);
            Assert.That(
                result.Value.First().FinalCode,
                Does.Contain("function twoSum(nums, target)")
            );
            Assert.That(result.Value.First().FunctionName, Is.EqualTo("twoSum"));
            Assert.That(result.Value.First().Inputs, Is.EqualTo("1,2"));
            Assert.That(result.Value.First().ExpectedOutput, Is.EqualTo("[0,1]"));
        });
    }

    [Test]
    public void Build_replaces_USER_CODE_placeholder_in_template()
    {
        var code = "function add(a, b) { return a + b; }";
        var contexts = new List<CodeBuilderContext>
        {
            new()
            {
                Code = code,
                Template = "{{USER_CODE}}",
                FunctionName = "add",
                Inputs = "1",
                ExpectedOutput = "2",
            },
        };

        var result = _sut.Build(contexts);

        Assert.That(result.Value.First().FinalCode, Is.EqualTo(code));
    }

    [Test]
    public void Build_replaces_FUNCTION_NAME_placeholder_in_template()
    {
        var contexts = new List<CodeBuilderContext>
        {
            new()
            {
                Code = "function myFunc() {}",
                Template = "{{FUNCTION_NAME}}()",
                FunctionName = "myFunc",
                Inputs = "1",
                ExpectedOutput = "result",
            },
        };

        var result = _sut.Build(contexts);

        Assert.That(result.Value.First().FinalCode, Is.EqualTo("myFunc()"));
    }

    [Test]
    public void Build_returns_invalid_when_code_is_empty()
    {
        var contexts = new List<CodeBuilderContext>
        {
            new()
            {
                Code = "",
                Template = "{{USER_CODE}}",
                FunctionName = "test",
                Inputs = "1",
                ExpectedOutput = "result",
            },
        };

        var result = _sut.Build(contexts);

        Assert.That(result.IsInvalid(), Is.True);
    }

    [Test]
    public void Build_returns_invalid_when_function_name_is_empty()
    {
        var contexts = new List<CodeBuilderContext>
        {
            new()
            {
                Code = "function test() {}",
                Template = "{{USER_CODE}}",
                FunctionName = "",
                Inputs = "1",
                ExpectedOutput = "result",
            },
        };

        var result = _sut.Build(contexts);

        Assert.That(result.IsInvalid(), Is.True);
    }

    [Test]
    public void Build_returns_invalid_when_template_is_empty()
    {
        var contexts = new List<CodeBuilderContext>
        {
            new()
            {
                Code = "function test() {}",
                Template = "",
                FunctionName = "test",
                Inputs = "1",
                ExpectedOutput = "result",
            },
        };

        var result = _sut.Build(contexts);

        Assert.That(result.IsInvalid(), Is.True);
    }

    [Test]
    public void Build_returns_success_with_multiple_contexts()
    {
        var contexts = new List<CodeBuilderContext>
        {
            new()
            {
                Code = "function a() {}",
                Template = "{{USER_CODE}}",
                FunctionName = "a",
                Inputs = "1",
                ExpectedOutput = "1",
            },
            new()
            {
                Code = "function b() {}",
                Template = "{{USER_CODE}}",
                FunctionName = "b",
                Inputs = "2",
                ExpectedOutput = "2",
            },
        };

        var result = _sut.Build(contexts);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Exactly(2).Items);
        });
    }

    [Test]
    public void Build_returns_success_with_empty_contexts()
    {
        var result = _sut.Build([]);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        });
    }

    [Test]
    public void Build_resolves_number_input_parser_for_number_input_type()
    {
        var contexts = new List<CodeBuilderContext>
        {
            new()
            {
                Code = "function f() {}",
                Template = "{{INPUT_PARSER}}",
                FunctionName = "f",
                Inputs = "42",
                ExpectedOutput = "42",
                InputTypeName = "number",
            },
        };

        var result = _sut.Build(contexts);

        Assert.That(
            result.Value.First().FinalCode,
            Is.EqualTo("const value = parseInt(data.toString(), 10);")
        );
    }

    [Test]
    public void Build_resolves_array_number_input_parser_for_array_number_input_type()
    {
        var contexts = new List<CodeBuilderContext>
        {
            new()
            {
                Code = "function f() {}",
                Template = "{{INPUT_PARSER}}",
                FunctionName = "f",
                Inputs = "1,2,3",
                ExpectedOutput = "6",
                InputTypeName = "array:number",
            },
        };

        var result = _sut.Build(contexts);

        Assert.That(
            result.Value.First().FinalCode,
            Is.EqualTo("const value = data.toString().split(',').map(Number);")
        );
    }

    [Test]
    public void Build_resolves_string_input_parser_for_string_input_type()
    {
        var contexts = new List<CodeBuilderContext>
        {
            new()
            {
                Code = "function f() {}",
                Template = "{{INPUT_PARSER}}",
                FunctionName = "f",
                Inputs = "hello",
                ExpectedOutput = "hello",
                InputTypeName = "string",
            },
        };

        var result = _sut.Build(contexts);

        Assert.That(
            result.Value.First().FinalCode,
            Is.EqualTo("const value = data.toString().trim();")
        );
    }
}
