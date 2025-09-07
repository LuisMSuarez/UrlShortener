namespace UrlShortenerApi.Services.Contracts;

/// <summary>
/// Represents a custom exception used to signal service-level errors with a specific result code.
/// </summary>
public class ServiceException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a result code,
    /// optional error message, and optional inner exception.
    /// </summary>
    /// <param name="resultCode">The result code representing the type of service error.</param>
    /// <param name="message">An optional message describing the error.</param>
    /// <param name="innerException">An optional inner exception that caused this error.</param>
    public ServiceException(ServiceResultCode resultCode, string? message = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ResultCode = resultCode;
    }

    /// <summary>
    /// Gets the result code associated with this service exception.
    /// </summary>
    public ServiceResultCode ResultCode { get; }
}
