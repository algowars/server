using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Clients;
using ApplicationCore.Logging;
using Ardalis.Result;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.CodeExecution.Judge0;

public sealed partial class Judge0Client(
    HttpClient httpClient,
    IOptions<Judge0Options> judge0Options,
    JsonSerializerOptions jsonOptions,
    ILogger<Judge0Client> logger
) : IJudge0Client
{
    private readonly ILogger<Judge0Client> _logger = logger;
    private readonly Judge0Options _judge0Options = judge0Options.Value;
    private const int BatchSize = 20;

    public async Task<Result<List<Judge0SubmissionResponse>>> GetAsync(
        IEnumerable<Guid> tokens,
        CancellationToken cancellationToken,
        IEnumerable<string>? fields = null
    )
    {
        var tokenList = tokens.ToList();
        LogGetStarted(tokenList.Count);
        var sw = Stopwatch.StartNew();

        try
        {
            var allSubmissions = new List<Judge0SubmissionResponse>();

            foreach (var batch in tokenList.Chunk(BatchSize))
            {
                var query = new Dictionary<string, string?>()
                {
                    ["tokens"] = string.Join(",", batch),
                    ["fields"] = fields is not null ? string.Join(",", fields) : "*",
                };

                string uri = QueryHelpers.AddQueryString("submissions/batch", query);

                var response = await httpClient.GetAsync(uri, cancellationToken);

                response.EnsureSuccessStatusCode();

                var batchResult = await response.Content.ReadFromJsonAsync<Judge0BatchGetResponse>(
                    jsonOptions,
                    cancellationToken
                );

                if (batchResult?.Submissions is { Count: > 0 })
                {
                    allSubmissions.AddRange(batchResult.Submissions);
                }
            }

            if (allSubmissions.Count == 0)
            {
                return Result.Error("No submissions found");
            }

            LogGetCompleted(tokenList.Count, sw.ElapsedMilliseconds);
            return Result.Success(allSubmissions);
        }
        catch (HttpRequestException ex)
        {
            LogGetFailed(tokenList.Count, sw.ElapsedMilliseconds, ex);
            return Result.Error($"HTTP request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            LogGetFailed(tokenList.Count, sw.ElapsedMilliseconds, ex);
            return Result.Error($"JSON deserialization failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogGetFailed(tokenList.Count, sw.ElapsedMilliseconds, ex);
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result<List<Judge0SubmissionResponse>>> SubmitAsync(
        IEnumerable<Judge0SubmissionRequest> reqs,
        CancellationToken cancellationToken,
        IEnumerable<string>? fields = null
    )
    {
        var reqList = reqs.ToList();
        LogSubmitStarted(reqList.Count);
        var sw = Stopwatch.StartNew();

        try
        {
            var allResponses = new List<Judge0SubmissionResponse>();

            foreach (var batch in reqList.Chunk(BatchSize))
            {
                var query = new Dictionary<string, string?>()
                {
                    ["base64_encoded"] = _judge0Options.IsEncoded.ToString().ToLowerInvariant(),
                    ["fields"] = fields is not null ? string.Join(",", fields) : "*",
                };

                string uri = QueryHelpers.AddQueryString("submissions/batch", query);

                var payload = new Judge0BatchRequest { Submissions = batch };

                var response = await httpClient.PostAsJsonAsync(uri, payload, cancellationToken);

                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadFromJsonAsync<
                    List<Judge0SubmissionTokenOnlyResponse>
                >(jsonOptions, cancellationToken);

                if (body is { Count: > 0 })
                {
                    allResponses.AddRange(
                        body.Select(result => new Judge0SubmissionResponse
                        {
                            Token = result.Token,
                            Status = new Judge0StatusModel { Id = (int)SubmissionStatus.InQueue },
                        })
                    );
                }
            }

            if (allResponses.Count == 0)
            {
                return Result.Error("No submissions found");
            }

            LogSubmitCompleted(reqList.Count, sw.ElapsedMilliseconds);
            return Result.Success(allResponses);
        }
        catch (HttpRequestException ex)
        {
            LogSubmitFailed(reqList.Count, sw.ElapsedMilliseconds, ex);
            return Result.Error($"HTTP request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            LogSubmitFailed(reqList.Count, sw.ElapsedMilliseconds, ex);
            return Result.Error($"JSON deserialization failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogSubmitFailed(reqList.Count, sw.ElapsedMilliseconds, ex);
            return Result.Error(ex.Message);
        }
    }

    [LoggerMessage(EventId = LoggingEventIds.Judge0.SubmitStarted, Level = LogLevel.Information,
        Message = "Judge0: Submitting {count} submission(s)")]
    private partial void LogSubmitStarted(int count);

    [LoggerMessage(EventId = LoggingEventIds.Judge0.SubmitCompleted, Level = LogLevel.Information,
        Message = "Judge0: Submitted {count} submission(s) in {elapsedMs}ms")]
    private partial void LogSubmitCompleted(int count, long elapsedMs);

    [LoggerMessage(EventId = LoggingEventIds.Judge0.SubmitFailed, Level = LogLevel.Error,
        Message = "Judge0: Submit failed for {count} submission(s) after {elapsedMs}ms")]
    private partial void LogSubmitFailed(int count, long elapsedMs, Exception ex);

    [LoggerMessage(EventId = LoggingEventIds.Judge0.GetStarted, Level = LogLevel.Information,
        Message = "Judge0: Polling {count} token(s)")]
    private partial void LogGetStarted(int count);

    [LoggerMessage(EventId = LoggingEventIds.Judge0.GetCompleted, Level = LogLevel.Information,
        Message = "Judge0: Polled {count} token(s) in {elapsedMs}ms")]
    private partial void LogGetCompleted(int count, long elapsedMs);

    [LoggerMessage(EventId = LoggingEventIds.Judge0.GetFailed, Level = LogLevel.Error,
        Message = "Judge0: Poll failed for {count} token(s) after {elapsedMs}ms")]
    private partial void LogGetFailed(int count, long elapsedMs, Exception ex);
}