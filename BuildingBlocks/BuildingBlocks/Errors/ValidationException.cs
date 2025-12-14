using System;
using System.Collections.Generic;

namespace BuildingBlocks.Errors
{
    /// <summary>
    ///     Exception thrown when validation fails.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(IReadOnlyDictionary<string, string[]> errors)
            : base("Validation failed")
        {
            Errors = errors;
        }

        public ValidationException(IReadOnlyDictionary<string, string[]> errors, Exception innerException)
            : base("Validation failed", innerException)
        {
            Errors = errors;
        }

        public ValidationException(string propertyName, string errorMessage)
            : base("Validation failed")
        {
            Errors = new Dictionary<string, string[]> { [propertyName] = new[] { errorMessage } };
        }

        public ValidationException(string message) : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }
}
