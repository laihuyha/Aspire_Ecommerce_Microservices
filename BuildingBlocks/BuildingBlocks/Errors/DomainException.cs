using System;

namespace BuildingBlocks.Errors
{
    /// <summary>
    ///     Exception thrown when a domain business rule is violated.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }

        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
