﻿using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;
using SharedKernel;

namespace Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId =>
        httpContextAccessor
            .HttpContext?
            .User
            .GetUserId() ??
        throw new UnauthorizedException("User context is unavailable");

    public bool IsAuthenticated =>
        httpContextAccessor
            .HttpContext?
            .User
            .Identity?
            .IsAuthenticated ??
        throw new UnauthorizedException("User context is unavailable");
}
