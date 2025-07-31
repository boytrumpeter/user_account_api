namespace DocumentProcessing.Functions.Infrastructure;

/// <summary>
/// Marker interface for commands that return a result
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command</typeparam>
public interface ICommand<TResult>
{
}

/// <summary>
/// Marker interface for commands that don't return a result
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Marker interface for queries that return a result
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query</typeparam>
public interface IQuery<TResult>
{
}