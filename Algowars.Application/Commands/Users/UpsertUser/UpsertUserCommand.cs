using Algowars.Application.Commands;
using Algowars.Application.Users.Dtos;
using MediatR;

namespace Algowars.Application.Commands.Users.UpsertUser;

internal sealed record UpsertUserCommand(string Sub, string? Username, string? ImageUrl, string? Bio) : ICommand<Unit>;