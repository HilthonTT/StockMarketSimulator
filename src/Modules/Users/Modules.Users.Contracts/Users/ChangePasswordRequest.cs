﻿namespace Modules.Users.Contracts.Users;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
