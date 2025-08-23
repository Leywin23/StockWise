using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json.Serialization;

namespace StockWise.Response
{
    public sealed class ApiError
    {
        public int Status { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string TraceId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object> Errors { get; init; }

        public static ApiError From(Exception ex, int status, HttpContext ctx, string title = null) => new ApiError
        {
            Status = status,
            Title = title ?? ReasonPhrases.GetReasonPhrase(status),
            Detail = ex.Message,
            TraceId = ctx.TraceIdentifier
        };
    }
}
