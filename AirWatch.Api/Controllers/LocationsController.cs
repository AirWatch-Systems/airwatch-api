using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.DTOs.Location;
using AirWatch.Api.DTOs.Common;
using AirWatch.Api.DTOs.User;
using AirWatch.Api.Models.Entities;
using AirWatch.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly ISearchHistoryRepository _searchHistoryRepository;
        private readonly ILogger<LocationsController> _logger;

        public LocationsController(
            ISearchHistoryRepository searchHistoryRepository,
            ILogger<LocationsController> logger)
        {
            _searchHistoryRepository = searchHistoryRepository;
            _logger = logger;
        }

        /// <summary>
        /// Searches for locations by text query.
        /// This endpoint would typically integrate with external geocoding services like Google Maps API.
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(LocationSearchResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<ActionResult<LocationSearchResponse>> SearchLocations(
            [FromQuery] LocationSearchRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(ValidationProblem(ModelState));
            }

            // TODO: Integrate with external geocoding service (Google Maps, OpenStreetMap, etc.)
            // For now, return mock data
            var mockResults = new List<LocationResultDto>
            {
                new LocationResultDto
                {
                    Name = $"Resultado para '{request.Query}'",
                    Latitude = -23.5505m,
                    Longitude = -46.6333m,
                    Address = "São Paulo, SP, Brasil",
                    PlaceId = "mock_place_id_1"
                },
                new LocationResultDto
                {
                    Name = $"Outro resultado para '{request.Query}'",
                    Latitude = -22.9068m,
                    Longitude = -43.1729m,
                    Address = "Rio de Janeiro, RJ, Brasil",
                    PlaceId = "mock_place_id_2"
                }
            };

            // Log the search for analytics
            _logger.LogInformation("Location search performed for query: {Query}", request.Query);

            return Ok(new LocationSearchResponse
            {
                Query = request.Query,
                Results = mockResults,
                Total = mockResults.Count
            });
        }

        /// <summary>
        /// Gets markers for a specific region (bounding box).
        /// Useful for map visualization with pollution data points.
        /// </summary>
        [HttpGet("markers")]
        [ProducesResponseType(typeof(MarkersResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<ActionResult<MarkersResponse>> GetMarkers(
            [FromQuery] MarkersRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(ValidationProblem(ModelState));
            }

            // TODO: Implement actual marker retrieval based on pollution data
            // This would typically query pollution cache and return markers for visualization
            var mockMarkers = new List<RegionMarkerDto>
            {
                new RegionMarkerDto
                {
                    Id = Guid.NewGuid(),
                    Latitude = -23.5505m,
                    Longitude = -46.6333m,
                    AQI = 75,
                    PM25 = 25.5m,
                    PM10 = 35.2m,
                    LastUpdated = DateTime.UtcNow.AddMinutes(-30)
                },
                new RegionMarkerDto
                {
                    Id = Guid.NewGuid(),
                    Latitude = -22.9068m,
                    Longitude = -43.1729m,
                    AQI = 45,
                    PM25 = 15.8m,
                    PM10 = 22.1m,
                    LastUpdated = DateTime.UtcNow.AddMinutes(-15)
                }
            };

            return Ok(new MarkersResponse
            {
                Markers = mockMarkers,
                Total = mockMarkers.Count,
                Bounds = new
                {
                    North = request.North,
                    South = request.South,
                    East = request.East,
                    West = request.West
                }
            });
        }

        /// <summary>
        /// Records a location search in the user's search history.
        /// </summary>
        [HttpPost("search-history")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> RecordSearchHistory(
            [FromBody] LocationSearchRequest request,
            [FromQuery] decimal lat,
            [FromQuery] decimal lon,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return await Task.FromResult(ValidationProblem(ModelState));
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new ErrorResponse("Invalid user authentication"));
            }

            var searchEntry = new SearchHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LocationName = request.Query,
                Latitude = lat,
                Longitude = lon,
                SearchedAt = DateTime.UtcNow
            };

            await _searchHistoryRepository.AddAsync(searchEntry, ct);

            _logger.LogInformation("Search history recorded for user {UserId}: {LocationName} at ({Lat}, {Lon})",
                userId, request.Query, lat, lon);

            return Ok(new { message = "Search history recorded successfully" });
        }

        /// <summary>
        /// Gets the current user's search history.
        /// </summary>
        [HttpGet("search-history")]
        [ProducesResponseType(typeof(UserHistoryResponse), 200)]
        public async Task<ActionResult<UserHistoryResponse>> GetSearchHistory(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken ct = default)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new ErrorResponse("Invalid user authentication"));
            }

            skip = Math.Max(0, skip);
            take = Math.Clamp(take, 1, 100);

            var searchHistory = await _searchHistoryRepository.GetByUserAsync(userId, skip, take, ct);

            var items = searchHistory.Select(s => new UserSearchHistoryItemDto
            {
                Id = s.Id,
                LocationName = s.LocationName,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                SearchedAt = s.SearchedAt
            }).ToList();

            return Ok(new UserHistoryResponse
            {
                Items = items,
                Total = items.Count,
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
