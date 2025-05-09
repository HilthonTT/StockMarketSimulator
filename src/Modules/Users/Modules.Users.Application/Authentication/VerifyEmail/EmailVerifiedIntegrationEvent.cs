﻿using Application.Abstractions.Events;

namespace Modules.Users.Application.Authentication.VerifyEmail;

public sealed record EmailVerifiedIntegrationEvent(Guid Id, Guid UserId) : IIntegrationEvent;
