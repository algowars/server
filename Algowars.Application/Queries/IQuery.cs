using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Queries;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
