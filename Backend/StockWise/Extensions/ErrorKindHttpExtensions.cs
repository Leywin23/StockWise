using Microsoft.AspNetCore.Mvc;
using StockWise.Response;

namespace StockWise.Extensions
{
    public static class ErrorKindHttpExtensions
    {
        public static (int status, string title) ToHttp(this ErrorKind kind)
        {
            return kind switch
            {
                ErrorKind.BadRequest => (StatusCodes.Status400BadRequest, "Bad Request"),
                ErrorKind.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ErrorKind.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
                ErrorKind.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
                ErrorKind.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };
        }
        
    }
}
