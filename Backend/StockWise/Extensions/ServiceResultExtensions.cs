using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockWise.Application.Abstractions; 

namespace StockWise.Extensions
{
    public static class ServiceResultExtensions
    {
        public static IActionResult ToActionResult<T>(this ControllerBase c, ServiceResult<T> r)
        {
            if (r.IsSuccess)
            {
                return r.Value is null ? c.Ok() : c.Ok(r.Value);
            }


            return r.Error switch
            {
                ErrorKind.BadRequest => c.BadRequest(ApiError.From(
                                            new Exception(r.Message ?? "Bad request"),
                                            StatusCodes.Status400BadRequest,
                                            c.HttpContext)),

                ErrorKind.Unauthorized => c.Unauthorized(ApiError.From(
                                            new Exception(r.Message ?? "Unauthorized"),
                                            StatusCodes.Status401Unauthorized,
                                            c.HttpContext)),

                ErrorKind.Forbidden => c.Forbid(), 

                ErrorKind.NotFound => c.NotFound(ApiError.From(
                                            new Exception(r.Message ?? "Not found"),
                                            StatusCodes.Status404NotFound,
                                            c.HttpContext)),

                ErrorKind.Conflict => c.Conflict(ApiError.From(
                                            new Exception(r.Message ?? "Conflict"),
                                            StatusCodes.Status409Conflict,
                                            c.HttpContext)),

                ErrorKind.ServerError => c.StatusCode(StatusCodes.Status500InternalServerError, ApiError.From(
                                            new Exception(r.Message ?? "Unexpected error"),
                                            StatusCodes.Status500InternalServerError,
                                            c.HttpContext)),

                _ => c.StatusCode(StatusCodes.Status500InternalServerError, ApiError.From(
                        new Exception(r.Message ?? "Unexpected error"),
                        StatusCodes.Status500InternalServerError,
                        c.HttpContext))
            };
        }
    }
}
