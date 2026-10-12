namespace CargoTrack.DTO.DTOs.ReportDtos
{
    // Neden: Rapor tarih aralığı. İkisi de boşsa tüm zamanlar.
    public class ReportFilterDto
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}