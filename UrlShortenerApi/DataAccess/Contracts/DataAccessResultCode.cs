namespace UrlShortenerApi.DataAccess.Contracts
{
    public enum DataAccessResultCode
    {
        Success = 0,
        NotFound,
        Unauthorized,
        BadRequest,
        Conflict,
        InternalServerError,
    }
}
