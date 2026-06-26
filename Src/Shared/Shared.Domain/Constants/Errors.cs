using Shared.Domain.DTOs;

namespace Shared.Domain.Constants;

public class Errors
{
    #region Common Errors
    public static readonly ErrorDTO InternalServerError = new ErrorDTO("500", "An internal server error occurred.");
    public static readonly ErrorDTO NotFound = new ErrorDTO("404", "Not Found");
    public static readonly ErrorDTO BadRequest = new ErrorDTO("400", "Bad Request");
    public static readonly ErrorDTO Unauthorized = new ErrorDTO("401", "Unauthorized");
    public static readonly ErrorDTO Forbidden = new ErrorDTO("403", "Forbidden");
    #endregion

    #region Authentication Errors
    public static readonly ErrorDTO InvalidCredentials = new ErrorDTO("404", "Invalid credentials.");
    public static readonly ErrorDTO TokenExpired = new ErrorDTO("401", "The authentication token has expired.");
    public static readonly ErrorDTO AccountLocked = new ErrorDTO("423", "The account is locked.");
    #endregion

    public static ErrorDTO Exception(Exception ex) =>
 new("9998", ex.InnerException?.Message ?? ex.Message);
}
