using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Commands;

public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

public interface ICommand : IRequest<Result> { }