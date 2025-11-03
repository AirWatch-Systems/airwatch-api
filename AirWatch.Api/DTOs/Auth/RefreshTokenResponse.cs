namespace AirWatch.Api.DTOs.Auth;

/// <summary>
/// Response for refresh token containing new JWT and refresh token.
/// </summary>
public record RefreshTokenResponse(string Token, string RefreshToken, int ExpiresIn);