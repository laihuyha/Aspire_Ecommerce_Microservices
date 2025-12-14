using System;

namespace BuildingBlocks.Errors
{
    /// <summary>
    ///     Exception thrown when a requested resource is not found.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string resourceType, object resourceId)
            : base($"The {resourceType} with id '{resourceId}' was not found.")
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }

        public NotFoundException(string resourceType, object resourceId, Exception innerException)
            : base($"The {resourceType} with id '{resourceId}' was not found.", innerException)
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }

        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string ResourceType { get; } = string.Empty;
        public object ResourceId { get; } = new();
    }
}
