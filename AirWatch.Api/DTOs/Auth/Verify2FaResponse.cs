using System;

namespace AirWatch.Api.DTOs.Auth;

/// <summary>
/// Response for 2FA verification, containing the JWT.
/// </summary>
public record Verify2FaResponse(string Token, int ExpiresIn);
