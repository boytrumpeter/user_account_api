namespace DocumentProcessing.Functions.Infrastructure.CommandDispatcher;

/// <summary>
/// Handler for commands that don't return a result
/// </summary>
/// <typeparam name="TCommand">The type of command to handle</typeparam>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler for commands that return a result
/// </summary>
/// <typeparam name="TCommand">The type of command to handle</typeparam>
/// <typeparam name="TResult">The type of result returned</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}