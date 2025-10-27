using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessing.Functions.Infrastructure.QueryDispatcher;

/// <summary>
/// Implementation of query dispatcher using dependency injection to resolve handlers
/// </summary>
public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueryDispatcher> _logger;

    public QueryDispatcher(IServiceProvider serviceProvider, ILogger<QueryDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        _logger.LogDebug("Dispatching query: {QueryType} with result: {ResultType}", typeof(TQuery).Name, typeof(TResult).Name);

        var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for query type {typeof(TQuery).Name} with result type {typeof(TResult).Name}");
        }

        try
        {
            var result = await handler.HandleAsync(query, cancellationToken);
            _logger.LogDebug("Successfully handled query: {QueryType} with result: {ResultType}", typeof(TQuery).Name, typeof(TResult).Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling query: {QueryType} with result: {ResultType}", typeof(TQuery).Name, typeof(TResult).Name);
            throw;
        }
    }
}