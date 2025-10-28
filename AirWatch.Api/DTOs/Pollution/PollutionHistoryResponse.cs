using System;
using System.Collections.Generic;

namespace AirWatch.Api.DTOs.Pollution;

/// <summary>
/// Response for pollution history series.
/// </summary>
public class PollutionHistoryResponse
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int Hours { get; set; }
    public List<PollutionHistoryPointDto> Points { get; set; } = new();
    public int Total { get; set; }
}
