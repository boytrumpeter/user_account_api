namespace DocumentProcessing.Functions.Infrastructure.QueryDispatcher;

/// <summary>
/// Handler for queries that return a result
/// </summary>
/// <typeparam name="TQuery">The type of query to handle</typeparam>
/// <typeparam name="TResult">The type of result returned</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}