using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Clients;
using Ardalis.Result;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.CodeExecution.Judge0;

public sealed class Judge0Client(
    HttpClient httpClient,
    IOptions<Judge0Options> judge0Options,
    JsonSerializerOptions jsonOptions
) : IJudge0Client
{
    private readonly Judge0Options _judge0Options = judge0Options.Value;
    private const int BatchSize = 20;

    public async Task<Result<List<Judge0SubmissionResponse>>> GetAsync(
        IEnumerable<Guid> tokens,
        CancellationToken cancellationToken,
        IEnumerable<string>? fields = null
    )
    {
        try
        {
            var tokenList = tokens.ToList();
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

            return Result.Success(allSubmissions);
        }
        catch (HttpRequestException ex)
        {
            return Result.Error($"HTTP request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Result.Error($"JSON deserialization failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result<List<Judge0SubmissionResponse>>> SubmitAsync(
        IEnumerable<Judge0SubmissionRequest> reqs,
        CancellationToken cancellationToken,
        IEnumerable<string>? fields = null
    )
    {
        try
        {
            var reqList = reqs.ToList();
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

            return Result.Success(allResponses);
        }
        catch (HttpRequestException ex)
        {
            return Result.Error($"HTTP request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Result.Error($"JSON deserialization failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}