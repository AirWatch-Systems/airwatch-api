using System;

namespace AirWatch.Api.DTOs.Auth;

/// <summary>
/// Response for user registration.
/// </summary>
public record RegisterResponse(Guid Id, string Message);
