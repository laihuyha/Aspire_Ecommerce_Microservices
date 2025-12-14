using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildingBlocks.Common
{
    /// <summary>
    ///     Represents the result of an operation that can succeed or fail.
    /// </summary>
    public class Result
    {
        protected Result(bool isSuccess, string error = null, IEnumerable<string> errors = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            Errors = errors?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }
        public IReadOnlyCollection<string> Errors { get; }

        public static Result Success()
        {
            return new Result(true);
        }

        public static Result Failure(string error)
        {
            return new Result(false, error);
        }

        public static Result Failure(IEnumerable<string> errors)
        {
            return new Result(false, errors: errors);
        }

        public static Result Failure(string error, IEnumerable<string> errors)
        {
            return new Result(false, error, errors);
        }

        public static Result<TValue> Success<TValue>(TValue value)
        {
            return new Result<TValue>(value, true);
        }

        public static Result<TValue> Failure<TValue>(string error)
        {
            return new Result<TValue>(default, false, error);
        }

        public static Result<TValue> Failure<TValue>(IEnumerable<string> errors)
        {
            return new Result<TValue>(default, false, errors: errors);
        }

        public static Result<TValue> Failure<TValue>(string error, IEnumerable<string> errors)
        {
            return new Result<TValue>(default, false, error, errors);
        }
    }

    /// <summary>
    ///     Represents the result of an operation that can succeed with a value or fail.
    /// </summary>
    public class Result<TValue> : Result
    {
        private readonly TValue _value;

        internal Result(TValue value, bool isSuccess, string error = null, IEnumerable<string> errors = null)
            : base(isSuccess, error, errors)
        {
            _value = value;
        }

        public TValue Value
        {
            get
            {
                if (!IsSuccess)
                {
                    throw new InvalidOperationException("Value is not available for failure results");
                }

                return _value;
            }
        }

        public bool HasValue => IsSuccess && _value != null;

        public static implicit operator Result<TValue>(TValue value)
        {
            return new Result<TValue>(value, true);
        }

        public static implicit operator TValue(Result<TValue> result)
        {
            return result.Value;
        }
    }
}
