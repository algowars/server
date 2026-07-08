namespace Algowars.Api.Requests.Problem;

public sealed record GetProblemsPageableRequest(int Page, int Size, DateTime Timestamp);
