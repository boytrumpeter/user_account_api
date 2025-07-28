# Command Dispatcher Pattern Implementation

## Overview

We've replaced **MediatR** with a custom **Command Dispatcher Pattern** to achieve better control, reduced dependencies, and cleaner DDD implementation.

## Why Replace MediatR?

### ❌ Issues with MediatR:

1. **Heavy Dependency**: Large library for simple request/response patterns
2. **Generic Abstractions**: `IRequest<T>` doesn't clearly express domain intent
3. **Reflection Overhead**: Runtime type resolution for handlers
4. **Configuration Complexity**: Auto-registration can be error-prone
5. **Domain Pollution**: Framework concepts leak into domain layer

### ✅ Benefits of Custom Command Dispatcher:

1. **Lightweight**: Minimal, focused implementation
2. **Clear Intent**: `ICommand<T>` and `IQuery<T>` express purpose explicitly
3. **Performance**: Direct DI resolution without reflection
4. **Control**: Full control over registration and behavior
5. **Domain Purity**: Clean separation of concerns

## Architecture Components

### 1. **Command Interfaces**

```csharp
// Commands that don't return results
public interface ICommand { }

// Commands that return results  
public interface ICommand<TResult> { }

// Command handlers
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
```

### 2. **Query Interfaces**

```csharp
// Queries always return results
public interface IQuery<TResult> { }

// Query handlers
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
```

### 3. **Dispatchers**

```csharp
public interface ICommandDispatcher
{
    Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
    Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>;
}

public interface IQueryDispatcher
{
    Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>;
}
```

## Implementation Example

### **Command Example**

```csharp
// 1. Define Command
public class ProcessSubmissionCommand : ICommand<ProcessSubmissionResponse>
{
    public string BlobUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

// 2. Implement Handler
public class ProcessSubmissionCommandHandler : ICommandHandler<ProcessSubmissionCommand, ProcessSubmissionResponse>
{
    public async Task<ProcessSubmissionResponse> HandleAsync(ProcessSubmissionCommand command, CancellationToken cancellationToken = default)
    {
        // Business logic here
        return new ProcessSubmissionResponse();
    }
}

// 3. Register in DI
services.AddScoped<ICommandHandler<ProcessSubmissionCommand, ProcessSubmissionResponse>, ProcessSubmissionCommandHandler>();

// 4. Use in Function
var result = await _commandDispatcher.DispatchAsync<ProcessSubmissionCommand, ProcessSubmissionResponse>(command);
```

### **Query Example**

```csharp
// 1. Define Query
public class GetSubmissionStatusQuery : IQuery<SubmissionStatusResponse?>
{
    public Guid SubmissionId { get; set; }
}

// 2. Implement Handler
public class GetSubmissionStatusQueryHandler : IQueryHandler<GetSubmissionStatusQuery, SubmissionStatusResponse?>
{
    public async Task<SubmissionStatusResponse?> HandleAsync(GetSubmissionStatusQuery query, CancellationToken cancellationToken = default)
    {
        // Query logic here
        return new SubmissionStatusResponse();
    }
}

// 3. Register in DI
services.AddScoped<IQueryHandler<GetSubmissionStatusQuery, SubmissionStatusResponse?>, GetSubmissionStatusQueryHandler>();

// 4. Use in Function
var result = await _queryDispatcher.DispatchAsync<GetSubmissionStatusQuery, SubmissionStatusResponse?>(query);
```

## Key Differences from MediatR

| Aspect | MediatR | Custom Dispatcher |
|--------|---------|-------------------|
| **Interfaces** | `IRequest<T>`, `INotification` | `ICommand<T>`, `IQuery<T>` |
| **Registration** | Auto-scan assembly | Explicit registration |
| **Performance** | Reflection-based | Direct DI resolution |
| **Dependencies** | External package | Internal implementation |
| **Customization** | Limited | Full control |
| **Domain Clarity** | Generic request/response | Clear command/query intent |

## Benefits for DDD

### 1. **Clear Separation of Concerns**
- **Commands**: Write operations that change state
- **Queries**: Read operations that return data
- **No confusion** between the two responsibilities

### 2. **Explicit Registration**
```csharp
// Clear, explicit handler registration
services.AddScoped<ICommandHandler<ProcessSubmissionCommand, ProcessSubmissionResponse>, ProcessSubmissionCommandHandler>();
services.AddScoped<IQueryHandler<GetSubmissionStatusQuery, SubmissionStatusResponse?>, GetSubmissionStatusQueryHandler>();
```

### 3. **Type Safety**
- Compile-time verification of command/query contracts
- No runtime surprises from missing handlers
- IntelliSense support for all operations

### 4. **Performance**
- No reflection overhead
- Direct dependency injection resolution
- Minimal runtime cost

### 5. **Testability**
```csharp
// Easy to test individual handlers
var handler = new ProcessSubmissionCommandHandler(dependencies...);
var result = await handler.HandleAsync(command);
```

## Error Handling

```csharp
public class CommandDispatcher : ICommandDispatcher
{
    public async Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
    {
        var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResult>>();
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for command type {typeof(TCommand).Name}");
        }

        try
        {
            return await handler.HandleAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command: {CommandType}", typeof(TCommand).Name);
            throw;
        }
    }
}
```

## Migration from MediatR

### **Before (MediatR)**
```csharp
public class ProcessSubmissionCommand : IRequest<ProcessSubmissionResponse> { }

public class ProcessSubmissionCommandHandler : IRequestHandler<ProcessSubmissionCommand, ProcessSubmissionResponse>
{
    public async Task<ProcessSubmissionResponse> Handle(ProcessSubmissionCommand request, CancellationToken cancellationToken) { }
}

// Usage
var result = await _mediator.Send(command);
```

### **After (Custom Dispatcher)**
```csharp
public class ProcessSubmissionCommand : ICommand<ProcessSubmissionResponse> { }

public class ProcessSubmissionCommandHandler : ICommandHandler<ProcessSubmissionCommand, ProcessSubmissionResponse>
{
    public async Task<ProcessSubmissionResponse> HandleAsync(ProcessSubmissionCommand command, CancellationToken cancellationToken = default) { }
}

// Usage
var result = await _commandDispatcher.DispatchAsync<ProcessSubmissionCommand, ProcessSubmissionResponse>(command);
```

## Summary

The custom Command Dispatcher pattern provides:

1. **🎯 Clear Intent**: Commands vs Queries are explicit
2. **⚡ Better Performance**: No reflection overhead
3. **🔧 Full Control**: Custom behavior and error handling
4. **🧪 Easy Testing**: Simple, focused interfaces
5. **📦 Lightweight**: No external dependencies
6. **🏗️ DDD Compliant**: Clean architectural boundaries

This implementation aligns perfectly with DDD principles while providing better performance and maintainability than generic mediator patterns.