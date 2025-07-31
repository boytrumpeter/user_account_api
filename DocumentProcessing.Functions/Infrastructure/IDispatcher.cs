namespace DocumentProcessing.Functions.Infrastructure;

/// <summary>
/// Command dispatcher for handling commands
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatches a command that returns a result
    /// </summary>
    Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Dispatches a command that doesn't return a result
    /// </summary>
    Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query dispatcher for handling queries
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Dispatches a query and returns the result
    /// </summary>
    Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}