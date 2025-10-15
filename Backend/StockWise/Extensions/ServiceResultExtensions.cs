using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using StockWise.Helpers;

namespace StockWise.Extensions
{
    public static class ServiceResultExtensions
    {
        public static IActionResult ToActionResult<T>(this ControllerBase c, ServiceResult<T> result)
        {
            if (result.IsSuccess)
            {
                return c.Ok(result.Value);
            }

            var (status, title) = result.Error.ToHttp();

            if (status == StatusCodes.Status400BadRequest && result.Details is Dictionary<string, string[]> ve && ve.Count > 0)
            {
                var apiValidation = ApiError.Validation(c.HttpContext, ve);
                return new ObjectResult(apiValidation) { StatusCode = status };
            }

            var apiErr = new ApiError
            {
                Status = status,
                Title = title,
                Detail = result.Message ?? ReasonPhrases.GetReasonPhrase(status),
                TraceId = c.HttpContext.TraceIdentifier
            };

            return new ObjectResult(apiErr) { StatusCode = status };
        }
    }
}
