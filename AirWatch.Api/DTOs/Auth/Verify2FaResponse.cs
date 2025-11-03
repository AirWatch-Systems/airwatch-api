using System;

namespace AirWatch.Api.DTOs.Auth;

/// <summary>
/// Response for 2FA verification, containing the JWT and refresh token.
/// </summary>
public record Verify2FaResponse(string Token, string RefreshToken, int ExpiresIn);
