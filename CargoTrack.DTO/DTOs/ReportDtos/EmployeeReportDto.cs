namespace CargoTrack.DTO.DTOs.ReportDtos
{
    // Neden: Personel bazlı rapor satırı.
    public class EmployeeReportDto
    {
        public string FullName { get; set; } = string.Empty;
        public string BranchName { get; set; } = "-";
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        public int TotalOperations { get; set; }
        public int TodayOperations { get; set; }
        public double SuccessRatePercent { get; set; }   // Teslim / (teslim + başarısız)
    }
}