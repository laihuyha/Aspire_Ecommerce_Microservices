using MediatR;

namespace BuildingBlocks.CQRS
{
    /// <summary>
    ///     Handler interface for commands with response.
    /// </summary>
    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse> where TResponse : notnull;

    /// <summary>
    ///     Handler interface for commands without response.
    /// </summary>
    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Unit> where TCommand : ICommand<Unit>;
}
