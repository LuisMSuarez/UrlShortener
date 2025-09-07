namespace UrlShortenerApi.Services.Contracts;

/// <summary>
/// Represents standardized result codes for service operations.
/// Used to convey the outcome of a service call in a consistent and type-safe manner.
/// </summary>
public enum ServiceResultCode
{
    /// <summary>
    /// Indicates that the operation completed successfully.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Indicates that the request was malformed or invalid.
    /// </summary>
    BadRequest,

    /// <summary>
    /// Indicates that the operation could not be completed due to a conflict (e.g., duplicate entry).
    /// </summary>
    Conflict,

    /// <summary>
    /// Indicates that an unexpected error occurred on the server.
    /// </summary>
    InternalServerError
}
