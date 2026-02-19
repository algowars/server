using System.Net.Http.Json;
using System.Text.Json;
using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Clients;
using Ardalis.Result;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Infrastructure.CodeExecution.Judge0;

public sealed class Judge0Client(
    HttpClient httpClient,
    IOptions<Judge0Options> judge0Options,
    JsonSerializerOptions jsonOptions
) : IJudge0Client
{
    private readonly Judge0Options _judge0Options = judge0Options.Value;

    public async Task<Result<List<Judge0SubmissionResponse>>> GetAsync(
        IEnumerable<Guid> tokens,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new Dictionary<string, string?>() { ["tokens"] = string.Join(",", tokens) };

            string uri = QueryHelpers.AddQueryString("submissions/batch", query);

            var response = await httpClient.GetAsync(uri, cancellationToken);

            response.EnsureSuccessStatusCode();

            var batch = await response.Content.ReadFromJsonAsync<Judge0BatchGetResponse>(
                jsonOptions,
                cancellationToken
            );

            if (batch == null || batch.Submissions.Count == 0)
            {
                return Result.Error("No submissions found");
            }

            if (_judge0Options.IsEncoded)
            {
                foreach (var s in batch.Submissions)
                {
                    s.SourceCode = Decode(s.SourceCode);
                    s.Stdout = Decode(s.Stdout);
                    s.Stderr = Decode(s.Stderr);
                }
            }

            return Result.Success(batch.Submissions);
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
        CancellationToken cancellationToken
    )
    {
        try
        {
            var query = new Dictionary<string, string?>()
            {
                ["base64_encoded"] = _judge0Options.IsEncoded.ToString().ToLowerInvariant(),
                ["fields"] = "*",
            };

            string uri = QueryHelpers.AddQueryString("submissions/batch", query);

            var submissions = reqs.Select(r =>
                {
                    if (!_judge0Options.IsEncoded)
                    {
                        return r;
                    }

                    return new Judge0SubmissionRequest
                    {
                        LanguageId = r.LanguageId,
                        SourceCode = Encode(r.SourceCode),
                        StdIn = Encode(r.StdIn),
                        ExpectedOutput = Encode(r.ExpectedOutput),
                    };
                })
                .ToList();

            var payload = new Judge0BatchRequest { Submissions = submissions };

            var response = await httpClient.PostAsJsonAsync(uri, payload, cancellationToken);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<
                List<Judge0SubmissionTokenOnlyResponse>
            >(jsonOptions, cancellationToken);

            if (body == null || body.Count == 0)
            {
                return Result.Error("No submissions found");
            }

            return Result.Success(
                body.Select(result => new Judge0SubmissionResponse
                    {
                        Token = result.Token,
                        Status = new Judge0StatusModel { Id = (int)SubmissionStatus.InQueue },
                    })
                    .ToList()
            );
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

    private static string? Encode(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
    }

    private static string? Decode(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
    }
}
