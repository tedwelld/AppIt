using System;

namespace AppIt.Core.DTOs
{
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }

        public ServiceResponse() { }

        public ServiceResponse(T data, string message = "")
        {
            Data = data;
            Message = message;
            IsSuccess = true;
            Time = DateTime.UtcNow;
        }
    }
}
