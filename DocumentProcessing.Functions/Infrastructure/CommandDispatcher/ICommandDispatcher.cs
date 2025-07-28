namespace DocumentProcessing.Functions.Infrastructure.CommandDispatcher;

/// <summary>
/// Command dispatcher responsible for routing commands to their appropriate handlers
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatch a command that doesn't return a result
    /// </summary>
    /// <typeparam name="TCommand">The type of command</typeparam>
    /// <param name="command">The command to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;

    /// <summary>
    /// Dispatch a command that returns a result
    /// </summary>
    /// <typeparam name="TCommand">The type of command</typeparam>
    /// <typeparam name="TResult">The type of result</typeparam>
    /// <param name="command">The command to dispatch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the command execution</returns>
    Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>;
}