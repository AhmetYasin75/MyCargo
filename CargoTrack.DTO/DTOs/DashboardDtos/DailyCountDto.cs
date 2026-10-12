namespace CargoTrack.DTO.DTOs.DashboardDtos
{
    // Neden: "Son 7 günlük trafik" grafiğinin bir günü.
    public class DailyCountDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}