using System;

namespace CommonClass
{
    public class CommonResponse<T>
    {
        public CommonResponse(T data, string message, bool isSuccess)
        {
            Data = data;
            Message = message;
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }
    }
}
