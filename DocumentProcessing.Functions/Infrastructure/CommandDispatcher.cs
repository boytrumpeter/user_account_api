using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessing.Functions.Infrastructure;

/// <summary>
/// Default implementation of command dispatcher using service provider
/// </summary>
public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));
        
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod("HandleAsync");
        if (method == null)
            throw new InvalidOperationException($"HandleAsync method not found for {handlerType.Name}");

        var result = method.Invoke(handler, new object[] { command, cancellationToken });
        
        if (result is Task<TResult> task)
            return await task;
            
        throw new InvalidOperationException($"Handler method did not return expected Task<{typeof(TResult).Name}>");
    }

    public async Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
        
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod("HandleAsync");
        if (method == null)
            throw new InvalidOperationException($"HandleAsync method not found for {handlerType.Name}");

        var result = method.Invoke(handler, new object[] { command, cancellationToken });
        
        if (result is Task task)
            await task;
        else
            throw new InvalidOperationException("Handler method did not return expected Task");
    }
}