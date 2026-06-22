namespace Algowars.Api.Requests.User;

public sealed record UpsertUserRequest(string? Username, string? Picture, string? Bio);
