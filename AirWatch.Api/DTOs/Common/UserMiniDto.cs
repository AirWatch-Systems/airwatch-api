using System;

namespace AirWatch.Api.DTOs.Common;

/// <summary>
/// Minimal user information for presentation purposes.
/// </summary>
public readonly record struct UserMiniDto(Guid Id, string Name, string? AvatarUrl);
