namespace UrlShortenerApi.DataAccess.Contracts;

/// <summary>
/// Represents an exception thrown by the data access layer.
/// Encapsulates a <see cref="DataAccessResultCode"/> to indicate the nature of the failure.
/// </summary>
public class DataAccessException(DataAccessResultCode resultCode, string? message = null, Exception? innerException = null)
    : Exception(message, innerException)
{
    /// <summary>
    /// Gets the result code that categorizes the type of data access failure.
    /// </summary>
    public DataAccessResultCode ResultCode { get; } = resultCode;
}
