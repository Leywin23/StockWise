using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json.Serialization;

public sealed class ApiError
{
    public int Status { get; init; }
    public string Title { get; init; } = default!;
    public string Detail { get; init; } = default!;
    public string TraceId { get; init; } = default!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]>? Errors { get; init; }

    public static ApiError From(Exception ex, int status, HttpContext ctx, string? title = null) => new()
    {
        Status = status,
        Title = title ?? ReasonPhrases.GetReasonPhrase(status),
        Detail = ex.Message,                
        TraceId = ctx.TraceIdentifier
    };

    public static ApiError Validation(HttpContext ctx, Dictionary<string, string[]> errors) => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Validation Failed",
        Detail = "One or more validation errors occurred.",
        TraceId = ctx.TraceIdentifier,
        Errors = errors.Count > 0 ? errors : null
    };
}
