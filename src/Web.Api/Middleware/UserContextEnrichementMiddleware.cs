﻿using System.Diagnostics;
using System.Security.Claims;

namespace Web.Api.Middleware;

internal sealed class UserContextEnrichementMiddleware(ILogger<UserContextEnrichementMiddleware> logger) 
    : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            Activity.Current?.SetTag("user.id", userId);

            var data = new Dictionary<string, object>
            {
                ["UserId"] = userId,
            };

            using (logger.BeginScope(data))
            {
                return next(context);
            }
        }
        else
        {
            return next(context);
        }
    }
}
