using System;

namespace Annstore.Web.Infrastructure
{
    public class AppResponse
    {
        public bool Success { get; set; }

        public bool ModelIsInvalid { get; set; }

        public string Message { get; set; }

        public static AppResponse<T> SuccessResult<T>(T result)
        {
            var response = new AppResponse<T>
            {
                Success = true,
                Result = result
            };
            return response;
        }
        public static AppResponse SuccessResult()
        {
            var response = new AppResponse
            {
                Success = true
            };
            return response;
        }

        public static AppResponse<T> InvalidModelResult<T>(string message)
        {
            var response = new AppResponse<T>
            {
                Success = false,
                ModelIsInvalid = true,
                Message = message
            };
            return response;
        }

        public static AppResponse InvalidModelResult(string message)
        {
            var response = new AppResponse
            {
                Success = false,
                ModelIsInvalid = true,
                Message = message
            };
            return response;
        }

        public static AppResponse<T> ErrorResult<T>(string message)
        {
            var response = new AppResponse<T>
            {
                Success = false,
                Message = message
            };
            return response;
        }

        public static AppResponse ErrorResult(string message)
        {
            var response = new AppResponse
            {
                Success = false,
                Message = message
            };
            return response;
        }
    }

    [Serializable]
    public sealed class AppResponse<T> : AppResponse
    {
        public T Result { get; set; }
    }
}
