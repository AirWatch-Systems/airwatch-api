using System;

namespace AirWatch.Api.DTOs.Auth;

/// <summary>
/// Represents a pending two-factor authentication session stored in the cache.
/// This is an internal model and not exposed via the API.
/// </summary>
/// <param name="UserId">The ID of the user attempting to log in.</param>
/// <param name="Code">The 2FA code sent to the user.</param>
internal record Pending2Fa(Guid UserId, string Code);
