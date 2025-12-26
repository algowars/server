using Ardalis.Result;
using MediatR;

namespace ApplicationCore.Queries;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
