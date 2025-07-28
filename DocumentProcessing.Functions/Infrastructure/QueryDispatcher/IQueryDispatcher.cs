namespace DocumentProcessing.Functions.Infrastructure.QueryDispatcher;

/// <summary>
/// Query dispatcher responsible for routing queries to their appropriate handlers
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Dispatch a query that returns a result
    /// </summary>
    /// <typeparam name="TQuery">The type of query</typeparam>
    /// <typeparam name="TResult">The type of result</typeparam>
    /// <param name="query">The query to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the query execution</returns>
    Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>;
}