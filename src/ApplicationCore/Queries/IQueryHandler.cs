using Ardalis.Result;
using MediatR;

namespace ApplicationCore.Queries;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
