using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using StockWise.Infrastructure.Persistence;
using StockWise.Models;
using System.IdentityModel.Tokens.Jwt;

namespace StockWise.Middleware
{
    public class JwtRevocationMiddleware
    {
        private readonly RequestDelegate _next;
        public JwtRevocationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext ctx, StockWiseDb db)
        {
            var user = ctx.User;
            if(user?.Identity?.IsAuthenticated == true)
            {
                var jti = user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if(!string.IsNullOrEmpty(jti) )
                {
                    var isRevoked = await db.RevokedTokens.AsNoTracking().AnyAsync(x => x.Jti == jti);
                    if(isRevoked)
                    {
                        if(!ctx.Response.HasStarted)
                        {
                            var apiErr = new ApiError
                            {
                                Status = StatusCodes.Status401Unauthorized,
                                Title = "Unauthorized",
                                Detail = "Token was revoked",
                                TraceId = ctx.TraceIdentifier
                            };
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.WriteAsJsonAsync(apiErr);
                            return;
                        };
                    }
                }

            }
            await _next(ctx);
        }
    }
}
