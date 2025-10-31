using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Application.Abstractions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal? user)
        {
            if (user is null) return null;

            return user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirst("sub")?.Value
                ?? user.FindFirst("uid")?.Value;
        }
    }
}
