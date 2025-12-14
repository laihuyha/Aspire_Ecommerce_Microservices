using MediatR;

namespace BuildingBlocks.CQRS
{
    /// <summary>
    ///     Marker interface for queries with response.
    /// </summary>
    public interface IQuery<out TResponse> : IRequest<TResponse> where TResponse : notnull
    {
    }
}
