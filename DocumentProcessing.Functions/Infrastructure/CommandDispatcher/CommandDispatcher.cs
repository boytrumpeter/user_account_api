using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessing.Functions.Infrastructure.CommandDispatcher;

/// <summary>
/// Implementation of command dispatcher using dependency injection to resolve handlers
/// </summary>
public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandDispatcher> _logger;

    public CommandDispatcher(IServiceProvider serviceProvider, ILogger<CommandDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        _logger.LogDebug("Dispatching command: {CommandType}", typeof(TCommand).Name);

        var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>();
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for command type {typeof(TCommand).Name}");
        }

        try
        {
            await handler.HandleAsync(command, cancellationToken);
            _logger.LogDebug("Successfully handled command: {CommandType}", typeof(TCommand).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command: {CommandType}", typeof(TCommand).Name);
            throw;
        }
    }

    public async Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        _logger.LogDebug("Dispatching command: {CommandType} with result: {ResultType}", typeof(TCommand).Name, typeof(TResult).Name);

        var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResult>>();
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for command type {typeof(TCommand).Name} with result type {typeof(TResult).Name}");
        }

        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            _logger.LogDebug("Successfully handled command: {CommandType} with result: {ResultType}", typeof(TCommand).Name, typeof(TResult).Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command: {CommandType} with result: {ResultType}", typeof(TCommand).Name, typeof(TResult).Name);
            throw;
        }
    }
}