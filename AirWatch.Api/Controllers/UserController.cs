using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.DTOs.Common;
using AirWatch.Api.DTOs.User;
using AirWatch.Api.Repositories;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserRepository userRepository,
            ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current user's profile information.
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserProfileResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<ActionResult<UserProfileResponse>> GetProfile(CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new ErrorResponse("Invalid user authentication"));
            }

            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                return NotFound(new ErrorResponse("User not found"));
            }

            var response = new UserProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Ok(response);
        }

        /// <summary>
        /// Updates the current user's profile information.
        /// </summary>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(UserProfileResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<ActionResult<UserProfileResponse>> UpdateProfile(
            [FromBody] UserUpdateRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new ErrorResponse("Invalid user authentication"));
            }

            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                return NotFound(new ErrorResponse("User not found"));
            }

            // Update user information
            user.Name = request.Name?.Trim() ?? user.Name;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, ct);

            _logger.LogInformation("User profile updated for user {UserId}", userId);

            var response = new UserProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Ok(response);
        }



        /// <summary>
        /// Gets user statistics and activity summary.
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(UserStatsResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        public async Task<ActionResult<UserStatsResponse>> GetUserStats(CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new ErrorResponse("Invalid user authentication"));
            }

            var user = await _userRepository.GetByIdAsync(userId, ct);
            var daysSinceRegistration = user != null ? (DateTime.UtcNow - user.CreatedAt).Days : 0;

            var response = new UserStatsResponse
            {
                TotalSearches = 0,
                TotalFeedbacks = 0,
                AverageRating = 0,
                DaysSinceRegistration = daysSinceRegistration,
                LastActivity = user?.CreatedAt
            };

            return Ok(response);
        }

        /// <summary>
        /// Changes the current user's password.
        /// </summary>
        [HttpPost("change-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new ErrorResponse("Invalid user authentication"));
            }

            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                return NotFound(new ErrorResponse("User not found"));
            }

            if (!BCrypt.Net.BCrypt.EnhancedVerify(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Invalid current password provided for user: {UserId}", userId);
                return BadRequest(new ErrorResponse("Current password is incorrect"));
            }

            user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.NewPassword, 12);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, ct);
            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

            return Ok(new { message = "Password changed successfully" });
        }

        /// <summary>
        /// Deletes the current user's account and all associated data.
        /// </summary>
        [HttpDelete("account")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> DeleteAccount(CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new ErrorResponse("Invalid user authentication"));
            }

            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                return NotFound(new ErrorResponse("User not found"));
            }

            // TODO: Implement cascade delete for user data
            // This would typically involve deleting:
            // - User's search history
            // - User's feedbacks
            // - User's account

            _logger.LogWarning("Account deletion requested for user {UserId} - implementation needed", userId);

            return Ok(new { message = "Account deletion requested. This feature requires implementation." });
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return Guid.Empty;
        }
    }
}
