using System.Diagnostics.CodeAnalysis;

namespace ClientNetLib.Types
{
    public class Result<TValue, TErrorValue>
    {
        [NotNull]
        private readonly TValue _value;
        [NotNull]
        private readonly TErrorValue _errorValue;

        private readonly bool _success;

        internal Result(bool success, [DisallowNull] TValue value, [DisallowNull] TErrorValue errValue)
        {
            if (success && value is null)
            {
                throw new ArgumentNullException(nameof(value), "Success value cannot be null.");
            }

            if (!success && errValue is null)
            {
                throw new ArgumentNullException(nameof(value), "Error value cannot be null.");
            }

            _value = value;
            _errorValue = errValue;
            _success = success;
        }

        public bool IsSuccess => _success;
        public bool IsError => !_success;

        [NotNull]
        public TValue GetValue
        {
            get
            {
                if (IsError)
                {
                    throw new InvalidOperationException("Cannot get the value from a failed result.");
                }

                return _value;
            }
        }

        [NotNull]
        public TErrorValue GetError
        {
            get
            {
                if (IsSuccess)
                {
                    throw new InvalidOperationException("Cannot get the error from a successful result.");
                }

                return _errorValue;
            }
        }
    }

    public static class Result
    {
        public static Result<TVal, TErrVal> Ok<TVal, TErrVal>(TVal success)
        {
            if (success == null)
            {
                throw new ArgumentNullException(nameof(success), "Success value cannot be null.");
            }

            return new Result<TVal, TErrVal>(true, success, default!);
        }

        public static Result<TVal, TErrVal> Error<TVal, TErrVal>(TErrVal error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error), "Error value cannot be null.");
            }

            return new Result<TVal, TErrVal>(false, default!, error);
        }
    }
}
