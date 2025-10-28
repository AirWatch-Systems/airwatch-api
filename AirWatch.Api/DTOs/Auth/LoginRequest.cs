using System.ComponentModel.DataAnnotations;

namespace AirWatch.Api.DTOs.Auth;

/// <summary>
/// Request body for login (step 1).
/// Endpoint: POST /api/auth/login
/// </summary>
public record LoginRequest(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email,

    [Required(ErrorMessage = "Password is required")]
    string Password
);
