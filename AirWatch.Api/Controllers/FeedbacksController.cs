using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.DTOs.Feedback;
using AirWatch.Api.DTOs.Common;
using AirWatch.Api.Models.Entities;
using AirWatch.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<FeedbacksController> _logger;

        public FeedbacksController(
            IFeedbackRepository feedbackRepository,
            IUserRepository userRepository,
            ILogger<FeedbacksController> logger)
        {
            _feedbackRepository = feedbackRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new feedback entry for air quality at a specific location.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(FeedbackCreateResponse), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 429)]
        public async Task<ActionResult<FeedbackCreateResponse>> CreateFeedback(
            [FromBody] FeedbackCreateRequest request,
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

            // Check if user has submitted feedback in this region within the last 4 hours
            var lastFeedback = await _feedbackRepository.GetLatestByUserInRegionAsync(
                userId, request.Lat, request.Lon, 1.0, ct); // 1km radius

            if (lastFeedback != null)
            {
                var timeSinceLastFeedback = DateTime.UtcNow - lastFeedback.CreatedAt;
                if (timeSinceLastFeedback.TotalHours < 4)
                {
                    var remainingTime = TimeSpan.FromHours(4) - timeSinceLastFeedback;
                    var remainingMinutes = (int)Math.Ceiling(remainingTime.TotalMinutes);
                    
                    return StatusCode(429, new ErrorResponse(
                        $"Você já enviou um feedback nesta região recentemente. Tente novamente em {remainingMinutes} minutos."));
                }
            }

            var feedback = new Feedback
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Latitude = request.Lat,
                Longitude = request.Lon,
                Rating = request.Rating,
                Comment = request.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _feedbackRepository.AddAsync(feedback, ct);

            _logger.LogInformation("New feedback created by user {UserId} at location ({Lat}, {Lon}) with rating {Rating}",
                userId, request.Lat, request.Lon, request.Rating);

            return CreatedAtAction(
                nameof(GetFeedback),
                new { id = feedback.Id },
                new FeedbackCreateResponse(feedback.Id, "Feedback created successfully"));
        }

        /// <summary>
        /// Gets a specific feedback by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FeedbackItemDto), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<ActionResult<FeedbackItemDto>> GetFeedback(Guid id, CancellationToken ct)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(id, ct);
            if (feedback == null)
            {
                return NotFound(new ErrorResponse("Feedback not found"));
            }

            return Ok(new FeedbackItemDto
            {
                Id = feedback.Id,
                UserId = feedback.UserId,
                Latitude = feedback.Latitude,
                Longitude = feedback.Longitude,
                Rating = feedback.Rating,
                Comment = feedback.Comment,
                CreatedAt = feedback.CreatedAt
            });
        }

        /// <summary>
        /// Gets feedbacks for the current user with pagination.
        /// </summary>
        [HttpGet("my")]
        [ProducesResponseType(typeof(FeedbackListResponse), 200)]
        public async Task<ActionResult<FeedbackListResponse>> GetMyFeedbacks(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken ct = default)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new ErrorResponse("Invalid user authentication"));
            }

            var skipValue = Math.Max(0, skip);
            var takeValue = Math.Max(1, Math.Min(take, 100));

            var feedbacks = await _feedbackRepository.GetByUserAsync(userId, skipValue, takeValue, ct);

            var items = feedbacks.Select(f => new FeedbackItemDto
            {
                Id = f.Id,
                UserId = f.UserId,
                Latitude = f.Latitude,
                Longitude = f.Longitude,
                Rating = f.Rating,
                Comment = f.Comment,
                CreatedAt = f.CreatedAt
            }).ToList();

            var response = new FeedbackListResponse
            {
                Items = items,
                Total = items.Count,
                Skip = skipValue,
                Take = takeValue
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets recent feedbacks near a specific location.
        /// </summary>
        [HttpGet("near")]
        [ProducesResponseType(typeof(FeedbackListResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<ActionResult<FeedbackListResponse>> GetFeedbacksNearLocation(
            [FromQuery] decimal lat,
            [FromQuery] decimal lon,
            [FromQuery] double radius = 5.0,
            [FromQuery] int hours = 24,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken ct = default)
        {
            if (lat < -90 || lat > 90)
            {
                ModelState.AddModelError(nameof(lat), "Latitude must be between -90 and 90");
            }

            if (lon < -180 || lon > 180)
            {
                ModelState.AddModelError(nameof(lon), "Longitude must be between -180 and 180");
            }

            if (radius <= 0 || radius > 100)
            {
                ModelState.AddModelError(nameof(radius), "Radius must be between 0 and 100 km");
            }

            if (hours <= 0 || hours > 168)
            {
                ModelState.AddModelError(nameof(hours), "Hours must be between 1 and 168");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 100);

            var feedbacks = await _feedbackRepository.GetRecentByLocationAsync(lat, lon, radius, hours, ct);

            // Apply pagination
            var paginatedFeedbacks = feedbacks.Skip(skip).Take(take);

            var items = paginatedFeedbacks.Select(f => new FeedbackItemDto
            {
                Id = f.Id,
                UserId = f.UserId,
                Latitude = f.Latitude,
                Longitude = f.Longitude,
                Rating = f.Rating,
                Comment = f.Comment,
                CreatedAt = f.CreatedAt
            }).ToList();

            var totalCount = feedbacks.Count;

            return Ok(new FeedbackListResponse
            {
                Items = items,
                Total = totalCount,
                Skip = skip,
                Take = take
            });
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
