using System.Net;
using System.Text;
using System.Text.Json;
using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Domain.Submissions;
using Infrastructure.CodeExecution.Judge0;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace UnitTests.Infrastructure.CodeExecution;

[TestFixture]
public sealed class Judge0ClientTests
{
    private Judge0Client _sut;
    private Mock<HttpMessageHandler> _handlerMock;
    private JsonSerializerOptions _jsonOptions;

    [SetUp]
    public void SetUp()
    {
        _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://judge0.test/"),
        };

        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var options = Options.Create(
            new Judge0Options
            {
                ApiKey = "123456",
                Host = "test-host",
                IsEncoded = false,
                BaseUrl = "https://judge0.test/",
            }
        );

        _sut = new Judge0Client(httpClient, options, _jsonOptions);
    }

    [Test]
    public async Task GetAsync_ShouldCallCorrectEndpoint_AndReturnResults()
    {
        var tokens = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var judge0Response = new
        {
            submissions = new[]
            {
                new { token = tokens[0], status = new { id = 1, description = "Accepted" } },
            },
        };

        string json = JsonSerializer.Serialize(judge0Response, _jsonOptions);

        HttpRequestMessage? capturedRequest = null;

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>(
                (req, _) =>
                {
                    capturedRequest = req;
                }
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                }
            );

        var result = await _sut.GetAsync(tokens, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            var submissions = result.Value.ToList();
            Assert.That(submissions, Has.Count.EqualTo(1));

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(
                capturedRequest!.RequestUri!.AbsolutePath,
                Is.EqualTo("/submissions/batch")
            );

            string query = capturedRequest.RequestUri.Query;
            Assert.That(query, Does.Contain("tokens="));
            Assert.That(query, Does.Contain(tokens[0].ToString()));
        }
    }

    [Test]
    public async Task GetAsync_WhenJsonIsInvalid_ReturnsJsonError()
    {
        var tokens = new[] { Guid.NewGuid() };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        "not-valid-json",
                        Encoding.UTF8,
                        "application/json"
                    ),
                }
            );

        var result = await _sut.GetAsync(tokens, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.Single(), Does.Contain("JSON deserialization failed"));
        }
    }

    [Test]
    public async Task GetAsync_WhenHttpRequestFails_ReturnsHttpError()
    {
        var tokens = new[] { Guid.NewGuid() };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var result = await _sut.GetAsync(tokens, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.Single(), Does.Contain("HTTP request failed"));
        }
        Assert.That(result.Errors.Single(), Does.Contain("Connection failed"));
    }

    [Test]
    public async Task SubmitAsync_ShouldCallCorrectEndpoint_AndReturnResults()
    {
        var submissions = new[]
        {
            new Judge0SubmissionRequest { LanguageId = 52, SourceCode = "print('hello')" },
        };

        var judge0Response = new[] { new { token = Guid.NewGuid() } };

        string json = JsonSerializer.Serialize(judge0Response, _jsonOptions);

        HttpRequestMessage? capturedRequest = null;

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>(
                (req, _) =>
                {
                    capturedRequest = req;
                }
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                }
            );

        var result = await _sut.SubmitAsync(submissions, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);

            var values = result.Value.ToList();
            Assert.That(values, Has.Count.EqualTo(1));

            Assert.That(values[0].Token, Is.EqualTo(judge0Response[0].token));
            Assert.That(values[0].Status.Id, Is.EqualTo((int)SubmissionStatus.InQueue));

            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest!.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(capturedRequest.RequestUri!.AbsolutePath, Is.EqualTo("/submissions/batch"));

            string query = capturedRequest.RequestUri.Query;
            Assert.That(query, Does.Contain("base64_encoded=false"));
            Assert.That(query, Does.Contain("fields=*"));
        }
    }
}
