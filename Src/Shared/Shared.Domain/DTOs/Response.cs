
namespace Shared.Domain.DTOs;

public class Response<T> where T : class
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<ErrorDTO> Errors { get; set; } = [];
    public T? Data { get; set; }

    public static Response<T> SuccessResponse(T data, string? msg = null)
    {
        return new()
        {
            Success = true,
            Message = msg,
            Data = data
        };
    }

    public static Response<T> SuccessResponse(string msg)
    {
        return new()
        {
            Success = true,
            Message = msg
        };
    }

    public static Response<T> FailureResponse(params ErrorDTO[] errors)
    {
        return new()
        {
            Success = false,
            Errors = errors.ToList()
        };
    }

    // New overload — same as above but also sets Message, needed by
    // GlobalExceptionHandler to carry a summary string alongside per-field errors.
    public static Response<T> FailureResponse(string msg, params ErrorDTO[] errors)
    {
        return new()
        {
            Success = false,
            Message = msg,
            Errors = errors.ToList()
        };
    }
}
