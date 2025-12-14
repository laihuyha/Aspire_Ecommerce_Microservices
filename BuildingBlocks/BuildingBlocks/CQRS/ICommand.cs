using MediatR;

namespace BuildingBlocks.CQRS
{
    /// <summary>
    ///     Marker interface for commands without response.
    /// </summary>
    public interface ICommand : IRequest<Unit>
    {
    }

    /// <summary>
    ///     Marker interface for commands with response.
    /// </summary>
    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }
}
