namespace UrlShortenerApi.DataAccess.Contracts
{
    public class DataAccessException(DataAccessResultCode resultCode, string? message = null, Exception? innerException = null)
        : Exception(message, innerException)
    {
        public DataAccessResultCode ResultCode { get; } = resultCode;
    }
}
