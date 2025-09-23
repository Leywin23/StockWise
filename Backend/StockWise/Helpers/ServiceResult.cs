using Microsoft.AspNetCore.Http.HttpResults;
using StockWise.Dtos.OrderDtos;

namespace StockWise.Helpers
{
    public enum ErrorKind
    {
        None = 0,
        BadRequest,
        NotFound,
        Unauthorized,
        Forbidden,
        Conflict,
        ServerError
    }
    public record ServiceResult<T>(bool IsSuccess, T? Value, ErrorKind Error, string? Message = null, object? Details = null)
    {
        public static ServiceResult<T> Ok(T value) 
            => new(true, value, ErrorKind.None);
        public static ServiceResult<T> BadRequest(string m, object? d = null) => new(false, default, ErrorKind.BadRequest, m, d);
        public static ServiceResult<T> Unauthorized(string m)
            => new(false, default, ErrorKind.Unauthorized, m);
        public static ServiceResult<T> Forbidden(string m)
            => new(false, default, ErrorKind.Forbidden, m);
        public static ServiceResult<T> NotFound(string? m)
            => new(false, default, ErrorKind.NotFound, m);
        public static ServiceResult<T> Conflict(string m)
            => new(false, default, ErrorKind.Conflict, m);

        public static ServiceResult<T> ServerError(string m)
            => new(false, default, ErrorKind.ServerError, m);
    }
}
