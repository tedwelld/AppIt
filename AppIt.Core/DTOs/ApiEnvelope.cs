using Microsoft.AspNetCore.Mvc;

namespace AppIt.Core.DTOs
{
    public class ApiEnvelope
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public ProblemDetails? Error { get; set; }
        public string? Message { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        public static ApiEnvelope Ok(object? data, string? message = null)
            => new()
            {
                Success = true,
                Data = data,
                Message = message
            };

        public static ApiEnvelope Fail(ProblemDetails error, string? message = null)
            => new()
            {
                Success = false,
                Error = error,
                Message = message
            };
    }
}
