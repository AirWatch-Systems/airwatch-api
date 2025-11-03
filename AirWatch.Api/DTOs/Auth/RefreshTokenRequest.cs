namespace AirWatch.Api.DTOs.Auth;

/// <summary>
/// Request for refreshing an expired JWT token.
/// </summary>
public record RefreshTokenRequest(string RefreshToken);