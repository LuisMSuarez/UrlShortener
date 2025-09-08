namespace UrlShortenerApi.DataAccess.Contracts
{
    /// <summary>
    /// Represents standardized result codes for data access operations.
    /// Used to classify outcomes and guide exception handling logic.
    /// </summary>
    public enum DataAccessResultCode
    {
        /// <summary>
        /// Indicates that the operation completed successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Indicates that the input was invalid or malformed.
        /// </summary>
        BadRequest,

        /// <summary>
        /// Indicates that a resource conflict occurred, such as a duplicate key.
        /// </summary>
        Conflict,

        /// <summary>
        /// Indicates that an unexpected error occurred within the data access layer.
        /// </summary>
        InternalServerError
    }
}
