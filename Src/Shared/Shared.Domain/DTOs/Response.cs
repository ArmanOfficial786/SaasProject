
namespace Shared.Domain.DTOs
{
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
    }
}
