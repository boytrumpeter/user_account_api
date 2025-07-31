using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessing.Functions.Infrastructure;

/// <summary>
/// Default implementation of query dispatcher using service provider
/// </summary>
public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));
        
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod("HandleAsync");
        if (method == null)
            throw new InvalidOperationException($"HandleAsync method not found for {handlerType.Name}");

        var result = method.Invoke(handler, new object[] { query, cancellationToken });
        
        if (result is Task<TResult> task)
            return await task;
            
        throw new InvalidOperationException($"Handler method did not return expected Task<{typeof(TResult).Name}>");
    }
}