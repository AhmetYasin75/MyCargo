namespace CargoTrack.DataAccess.Repositories.Reports
{
    // Neden: Personel bazlı rapor satırı (teslim, başarısız, operasyon sayıları).
    public class EmployeeReportRow
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string BranchName { get; set; } = "-";
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        public int TotalOperations { get; set; }
        public int TodayOperations { get; set; }
    }
}