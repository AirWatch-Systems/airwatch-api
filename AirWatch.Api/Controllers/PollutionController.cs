using System;
using System.Threading;
using System.Threading.Tasks;
using AirWatch.Api.DTOs.Pollution;
using AirWatch.Api.DTOs.Common;
using AirWatch.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AirWatch.Api.Services.Interfaces;

namespace AirWatch.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PollutionController : ControllerBase
    {
        private readonly IOpenWeatherMapService _openWeatherMapService;
        private readonly ILogger<PollutionController> _logger;

        public PollutionController(
            IOpenWeatherMapService openWeatherMapService,
            ILogger<PollutionController> logger)
        {
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

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var owmData = await _openWeatherMapService.GetCurrentPollutionAsync(lat, lon);
                if (owmData.List.Count > 0)
                {
                    var data = owmData.List[0];
                    var response = new PollutionCurrentResponse
                    {
                        Latitude = lat,
                        Longitude = lon,
                        AQI = data.Main.Aqi,
                        Pollutants = new PollutantsDto
                        {
                            PM25 = data.Components.Pm2_5,
                            PM10 = data.Components.Pm10,
                            CO = data.Components.Co,
                            NO2 = data.Components.No2,
                            SO2 = data.Components.So2,
                            O3 = data.Components.O3
                        },
                        LastUpdated = DateTime.UtcNow,
                        DataAge = TimeSpan.Zero
                    };

                    _logger.LogInformation("Pollution data retrieved for location ({Lat}, {Lon}) with AQI {AQI}",
                        lat, lon, data.Main.Aqi);

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch pollution data from OpenWeatherMap API for location ({Lat}, {Lon})", lat, lon);
            }

            return NotFound(new ErrorResponse("No pollution data available for this location"));
        }


    }
}
