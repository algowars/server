using System.Net.Http.Json;
using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Interfaces.Clients;
using Ardalis.Result;

namespace Infrastructure.CodeExecution.Judge0;

public sealed class Judge0Client(HttpClient http) : IJudge0Client
{
    public async Task<Result<Judge0SubmissionResponse>> SubmitAsync(
        Judge0SubmissionRequest req,
        CancellationToken ct
    )
    {
        try
        {
            string uri = "submissions?base64_encoded=false&wait=false";

            using var resp = await http.PostAsJsonAsync(uri, ToPayload(req), ct);
            if (!resp.IsSuccessStatusCode)
            {
                string body = await resp.Content.ReadAsStringAsync(ct);
                return Result.Error(body);
            }

            var token = await resp.Content.ReadFromJsonAsync<TokenOnly>(cancellationToken: ct);
            if (token is null)
            {
                return Result.Error("Judge0 returned empty token.");
            }

            return Result.Success(new Judge0SubmissionResponse { Token = token.token });
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<Judge0SubmissionResponse>>> SubmitAsync(
        IEnumerable<Judge0SubmissionRequest> reqs,
        CancellationToken ct
    )
    {
        var list = reqs?.ToList();
        if (list is null || list.Count == 0)
        {
            return Result.Error("There should be at least one submission in a batch.");
        }

        try
        {
            string uri = "submissions/batch?base64_encoded=false&wait=false";

            var payload = new Judge0BatchRequest
            {
                Submissions = list.Select(r => new Judge0SubmissionRequest()
                    {
                        LanguageId = r.LanguageId,
                        SourceCode = r.SourceCode,
                        StdIn = r.StdIn,
                        ExpectedOutput = r.ExpectedOutput,
                    })
                    .ToList(),
            };

            using var response = await http.PostAsJsonAsync(uri, payload, ct);
            string body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Error(body);
            }

            var tokens = await response.Content.ReadFromJsonAsync<List<TokenOnly>>(
                cancellationToken: ct
            );

            if (tokens is null || tokens.Count == 0)
            {
                return Result.Error("Judge0 returned empty token list.");
            }

            return Result.Success(
                tokens.Select(t => new Judge0SubmissionResponse { Token = t.token })
            );
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result<Judge0SubmissionResponse>> GetAsync(string token, CancellationToken ct)
    {
        try
        {
            string uri = $"submissions/{token}?base64_encoded=false";
            using var resp = await http.GetAsync(uri, ct);

            if (!resp.IsSuccessStatusCode)
            {
                string body = await resp.Content.ReadAsStringAsync(ct);
                return Result.Error(body);
            }

            var detail = await resp.Content.ReadFromJsonAsync<Judge0SubmissionResponse>(
                cancellationToken: ct
            );
            return detail is null
                ? Result.Error("Judge0 returned empty submission detail.")
                : Result.Success(detail);
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    public async Task<Result<IEnumerable<Judge0SubmissionResponse>>> GetAsync(
        IEnumerable<string> tokens,
        CancellationToken ct
    )
    {
        var list = tokens?.ToList();
        if (list is null || list.Count == 0)
        {
            return Result.Success(Enumerable.Empty<Judge0SubmissionResponse>());
        }

        try
        {
            string uri = $"submissions/batch?tokens={string.Join(",", list)}&base64_encoded=false";
            using var resp = await http.GetAsync(uri, ct);

            if (!resp.IsSuccessStatusCode)
            {
                string body = await resp.Content.ReadAsStringAsync(ct);
                return Result.Error(body);
            }

            var details = await resp.Content.ReadFromJsonAsync<Judge0SubmissionResponse[]>(
                cancellationToken: ct
            );

            if (details is null)
            {
                return Result.Error("Judge0 returned empty batch result.");
            }

            return Result.Success(
                details.Select(d => new Judge0SubmissionResponse { Token = d.Token })
            );
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }

    private static object ToPayload(Judge0SubmissionRequest req) =>
        new
        {
            language_id = req.LanguageId,
            source_code = req.SourceCode,
            stdin = req.StdIn,
            expected_output = req.ExpectedOutput,
        };

    private sealed record TokenOnly(string token);
}
