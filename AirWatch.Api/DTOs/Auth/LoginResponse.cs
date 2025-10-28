using System;

namespace AirWatch.Api.DTOs.Auth;

/// <summary>
/// Response for login (step 1).
/// If Requires2Fa = true, client must call verify-2fa with provided SessionId.
/// </summary>
public record LoginResponse(bool Requires2Fa, string SessionId);
