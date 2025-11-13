namespace AirWatch.Api.DTOs.External
{
    public class AirPollutionResponse
    {
        public Coord Coord { get; set; } = new();
        public List<PollutionData> List { get; set; } = new();
    }

    public class AirPollutionHistoryResponse
    {
        public Coord Coord { get; set; } = new();
        public List<PollutionData> List { get; set; } = new();
    }

    public class Coord
    {
        public decimal Lon { get; set; }
        public decimal Lat { get; set; }
    }

    public class PollutionData
    {
        public long Dt { get; set; }
        public Main Main { get; set; } = new();
        public Components Components { get; set; } = new();

        public DateTime GetDateTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(Dt).UtcDateTime;
        }
    }

    public class Main
    {
        public int Aqi { get; set; }
    }

    public class Components
    {
        public decimal Co { get; set; }
        public decimal No { get; set; }
        public decimal No2 { get; set; }
        public decimal O3 { get; set; }
        public decimal So2 { get; set; }
        public decimal Pm2_5 { get; set; }
        public decimal Pm10 { get; set; }
        public decimal Nh3 { get; set; }
    }
}