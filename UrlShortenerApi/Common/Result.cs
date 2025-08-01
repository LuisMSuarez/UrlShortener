namespace UrlShortenerApi.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? Error { get; }

        public ResultCode? Code { get; }

        private Result(bool isSuccess, T? value, string? error, ResultCode? code)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            Code = code;
        }

        public static Result<T> Success(T value) => new(true, value, null, ResultCode.Success);
        public static Result<T> Failure(ResultCode code, string error) => new(false, default, error, code);
    }
}
