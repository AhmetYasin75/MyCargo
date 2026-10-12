namespace CargoTrack.DTO.DTOs.ReportDtos
{
    // Neden: Rapor ekranının tüm verisi tek nesnede.
    public class ReportDto
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public int TotalCargoCount { get; set; }
        public int DeliveredCount { get; set; }
        public double AvgDeliveryHours { get; set; }
        public double OnTimeRatePercent { get; set; }
        public double ReturnRatePercent { get; set; }

        public string? MostSendingBranch { get; set; }
        public string? MostDeliveringBranch { get; set; }

        public List<BranchReportDto> Branches { get; set; } = new();
        public List<BranchReportDto> MostDelayedBranches { get; set; } = new();  // İlk 5, gecikmesi olanlar
        public List<EmployeeReportDto> Employees { get; set; } = new();
    }
}