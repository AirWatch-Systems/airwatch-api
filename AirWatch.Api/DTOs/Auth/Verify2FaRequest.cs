using System.ComponentModel.DataAnnotations;

namespace AirWatch.Api.DTOs.Auth;

/// <summary>
/// Request body for 2FA verification (step 2).
/// Endpoint: POST /api/auth/verify-2fa
/// </summary>
public record Verify2FaRequest(
    [Required(ErrorMessage = "SessionId is required")]
    string SessionId,

    [Required(ErrorMessage = "Token is required")]
    string Token
);
