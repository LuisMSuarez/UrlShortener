namespace UrlShortenerApi.Services.Contracts
{
    public enum ServiceResultCode
    {
        Success = 0,
        NotFound,
        Unauthorized,
        BadRequest,
        Conflict,
        InternalServerError,
    }
}