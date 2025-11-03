using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.DTOs.Pollution;
using AirWatch.Api.DTOs.Common;
using AirWatch.Api.Models.Entities;
using AirWatch.Api.Repositories;
using AirWatch.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PollutionController : ControllerBase
    {
        private readonly IPollutionCacheRepository _pollutionCacheRepository;
        private readonly IOpenWeatherMapService _openWeatherMapService;
        private readonly ILogger<PollutionController> _logger;

        public PollutionController(
            IPollutionCacheRepository pollutionCacheRepository,
            IOpenWeatherMapService openWeatherMapService,
            ILogger<PollutionController> logger)
        {
            _pollutionCacheRepository = pollutionCacheRepository;
            _openWeatherMapService = openWeatherMapService;
            _logger = logger;
        }

        /// <summary>
        /// Gets current pollution data for a specific location.
        /// </summary>
        [HttpGet("current")]
        [ProducesResponseType(typeof(PollutionCurrentResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<ActionResult<PollutionCurrentResponse>> GetCurrentPollution(
            [FromQuery] decimal lat,
            [FromQuery] decimal lon,
            [FromQuery] int maxAgeMinutes = 60,
            [FromQuery] double radiusKm = 2.0,
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

            if (maxAgeMinutes <= 0 || maxAgeMinutes > 1440)
            {
                ModelState.AddModelError(nameof(maxAgeMinutes), "Max age must be between 1 and 1440 minutes");
            }

            if (radiusKm <= 0 || radiusKm > 50)
            {
                ModelState.AddModelError(nameof(radiusKm), "Radius must be between 0 and 50 km");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var pollutionData = await _pollutionCacheRepository.GetLatestForLocationAsync(
                lat, lon, maxAgeMinutes, radiusKm, ct);

            if (pollutionData == null)
            {
                // Try to fetch from OpenWeatherMap API
                try
                {
                    var owmData = await _openWeatherMapService.GetCurrentPollutionAsync(lat, lon);
                    if (owmData.List.Count > 0)
                    {
                        var data = owmData.List[0];
                        pollutionData = new PollutionCache
                        {
                            Latitude = lat,
                            Longitude = lon,
                            AQI = data.Main.Aqi,
                            PM25 = data.Components.Pm2_5,
                            PM10 = data.Components.Pm10,
                            CO = data.Components.Co,
                            NO2 = data.Components.No2,
                            SO2 = data.Components.So2,
                            O3 = data.Components.O3,
                            FetchedAt = DateTime.UtcNow
                        };

                        // Cache the data
                        await _pollutionCacheRepository.AddAsync(pollutionData, ct);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch pollution data from OpenWeatherMap API for location ({Lat}, {Lon})", lat, lon);
                }

                if (pollutionData == null)
                {
                    _logger.LogWarning("No pollution data found for location ({Lat}, {Lon}) within {MaxAgeMinutes} minutes and {RadiusKm} km radius",
                        lat, lon, maxAgeMinutes, radiusKm);
                    return NotFound(new ErrorResponse("No pollution data available for this location"));
                }
            }

            var response = new PollutionCurrentResponse
            {
                Latitude = pollutionData.Latitude,
                Longitude = pollutionData.Longitude,
                AQI = pollutionData.AQI,
                Pollutants = new PollutantsDto
                {
                    PM25 = pollutionData.PM25,
                    PM10 = pollutionData.PM10,
                    CO = pollutionData.CO,
                    NO2 = pollutionData.NO2,
                    SO2 = pollutionData.SO2,
                    O3 = pollutionData.O3
                },
                LastUpdated = pollutionData.FetchedAt,
                DataAge = DateTime.UtcNow - pollutionData.FetchedAt
            };

            _logger.LogInformation("Pollution data retrieved for location ({Lat}, {Lon}) with AQI {AQI}",
                lat, lon, pollutionData.AQI);

            return Ok(response);
        }

        /// <summary>
        /// Gets pollution history for a specific location.
        /// </summary>
        [HttpGet("history")]
        [ProducesResponseType(typeof(PollutionHistoryResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<ActionResult<PollutionHistoryResponse>> GetPollutionHistory(
            [FromQuery] PollutionHistoryRequest request,
            CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var historyData = await _pollutionCacheRepository.GetHistoryAsync(
                request.Lat, request.Lon, request.Hours, ct);

            // If no cached history, try to fetch from OpenWeatherMap
            if (!historyData.Any())
            {
                try
                {
                    var owmHistory = await _openWeatherMapService.GetPollutionHistoryAsync(request.Lat, request.Lon, request.Hours);
                    if (owmHistory.List.Count > 0)
                    {
                        var newHistoryData = owmHistory.List.Select(data => new PollutionCache
                        {
                            Latitude = request.Lat,
                            Longitude = request.Lon,
                            AQI = data.Main.Aqi,
                            PM25 = data.Components.Pm2_5,
                            PM10 = data.Components.Pm10,
                            CO = data.Components.Co,
                            NO2 = data.Components.No2,
                            SO2 = data.Components.So2,
                            O3 = data.Components.O3,
                            FetchedAt = data.GetDateTime()
                        }).ToList();

                        // Cache the history data
                        foreach (var item in newHistoryData)
                        {
                            await _pollutionCacheRepository.AddAsync(item, ct);
                        }

                        historyData = newHistoryData;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch pollution history from OpenWeatherMap API for location ({Lat}, {Lon})", request.Lat, request.Lon);
                }
            }

            var points = historyData.Select(p => new PollutionHistoryPointDto
            {
                Timestamp = p.FetchedAt,
                AQI = p.AQI,
                PM25 = p.PM25,
                PM10 = p.PM10,
                CO = p.CO,
                NO2 = p.NO2,
                SO2 = p.SO2,
                O3 = p.O3
            }).OrderBy(p => p.Timestamp).ToList();

            var response = new PollutionHistoryResponse
            {
                Latitude = request.Lat,
                Longitude = request.Lon,
                Hours = request.Hours,
                Points = points,
                Total = points.Count
            };

            _logger.LogInformation("Pollution history retrieved for location ({Lat}, {Lon}) for {Hours} hours with {Count} data points",
                request.Lat, request.Lon, request.Hours, points.Count);

            return Ok(response);
        }

        /// <summary>
        /// Caches new pollution data for a location.
        /// This endpoint would typically be called by background services that fetch data from external APIs.
        /// </summary>
        [HttpPost("cache")]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> CachePollutionData(
            [FromBody] PollutionCacheRequest request,
            CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var pollutionCache = new PollutionCache
            {
                Id = Guid.NewGuid(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AQI = request.AQI,
                PM25 = request.PM25,
                PM10 = request.PM10,
                CO = request.CO,
                NO2 = request.NO2,
                SO2 = request.SO2,
                O3 = request.O3,
                FetchedAt = DateTime.UtcNow
            };

            await _pollutionCacheRepository.AddAsync(pollutionCache, ct);

            _logger.LogInformation("Pollution data cached for location ({Lat}, {Lon}) with AQI {AQI}",
                request.Latitude, request.Longitude, request.AQI);

            return CreatedAtAction(
                nameof(GetCurrentPollution),
                new { lat = request.Latitude, lon = request.Longitude },
                new { id = pollutionCache.Id, message = "Pollution data cached successfully" });
        }

        /// <summary>
        /// Gets pollution data for multiple locations (batch request).
        /// </summary>
        [HttpPost("batch")]
        [ProducesResponseType(typeof(PollutionBatchResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<ActionResult<PollutionBatchResponse>> GetBatchPollutionData(
            [FromBody] PollutionBatchRequest request,
            CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var results = new List<PollutionCurrentResponse>();

            foreach (var location in request.Locations)
            {
                var pollutionData = await _pollutionCacheRepository.GetLatestForLocationAsync(
                    location.Lat, location.Lon, request.MaxAgeMinutes, request.RadiusKm, ct);

                if (pollutionData != null)
                {
                    results.Add(new PollutionCurrentResponse
                    {
                        Latitude = pollutionData.Latitude,
                        Longitude = pollutionData.Longitude,
                        AQI = pollutionData.AQI,
                        Pollutants = new PollutantsDto
                        {
                            PM25 = pollutionData.PM25,
                            PM10 = pollutionData.PM10,
                            CO = pollutionData.CO,
                            NO2 = pollutionData.NO2,
                            SO2 = pollutionData.SO2,
                            O3 = pollutionData.O3
                        },
                        LastUpdated = pollutionData.FetchedAt,
                        DataAge = DateTime.UtcNow - pollutionData.FetchedAt
                    });
                }
            }

            return Ok(new PollutionBatchResponse
            {
                Results = results,
                Total = results.Count,
                Requested = request.Locations.Count
            });
        }
    }

    // Additional DTOs for pollution caching and batch requests
    public class PollutionCacheRequest
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int AQI { get; set; }
        public decimal PM25 { get; set; }
        public decimal PM10 { get; set; }
        public decimal CO { get; set; }
        public decimal NO2 { get; set; }
        public decimal SO2 { get; set; }
        public decimal O3 { get; set; }
    }

    public class PollutionBatchRequest
    {
        public List<CoordinatesDto> Locations { get; set; } = new();
        public int MaxAgeMinutes { get; set; } = 60;
        public double RadiusKm { get; set; } = 2.0;
    }

    public class PollutionBatchResponse
    {
        public List<PollutionCurrentResponse> Results { get; set; } = new();
        public int Total { get; set; }
        public int Requested { get; set; }
    }
}
