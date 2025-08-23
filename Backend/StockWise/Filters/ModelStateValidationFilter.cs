using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StockWise.Filters
{
    public sealed class ModelStateValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid) return;

            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(er => er.ErrorMessage).ToArray()
                );

            var apiError = new ApiError
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                TraceId = context.HttpContext.TraceIdentifier,
                Errors = errors
            };




            context.Result = new JsonResult(apiError)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
