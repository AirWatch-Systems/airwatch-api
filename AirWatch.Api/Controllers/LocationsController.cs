using System;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.DTOs.Location;
using AirWatch.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirWatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly ILogger<LocationsController> _logger;
        private readonly IGoogleMapsGeocodingService _geocodingService;

        public LocationsController(
            ILogger<LocationsController> logger,
            IGoogleMapsGeocodingService geocodingService)
        {
            _logger = logger;
            _geocodingService = geocodingService;
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

            var results = await _geocodingService.SearchAsync(request.Query, ct);

            _logger.LogInformation("Location search performed for query: {Query}", request.Query);

            return Ok(new LocationSearchResponse
            {
                Query = request.Query,
                Results = results,
                Total = results.Count
            });
        }


    }
}
