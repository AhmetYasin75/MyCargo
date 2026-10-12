namespace CargoTrack.DTO.DTOs.ReportDtos
{
    // Neden: Şube bazlı rapor satırı. Oranlar serviste hesaplanıp hazır gelir.
    public class BranchReportDto
    {
        public string BranchName { get; set; } = string.Empty;
        public int SentCount { get; set; }
        public int ReceivedCount { get; set; }
        public int DeliveredCount { get; set; }
        public int DelayedCount { get; set; }
        public int ReturnedCount { get; set; }
        public double AvgDeliveryHours { get; set; }
        public double DeliveryRatePercent { get; set; }  // Teslim / varış adresli
        public double OnTimeRatePercent { get; set; }    // Zamanında / teslim edilen
    }
}