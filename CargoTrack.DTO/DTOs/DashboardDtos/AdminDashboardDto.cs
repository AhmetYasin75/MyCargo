namespace CargoTrack.DTO.DTOs.DashboardDtos
{
    // Neden: Admin dashboard'unun tüm rakamları tek nesnede taşınır.
    public class AdminDashboardDto
    {
        public int TotalCargoCount { get; set; }
        public int ActiveCargoCount { get; set; }       // Teslim edilmemiş ve iadesi tamamlanmamış
        public int OverdueCargoCount { get; set; }      // Aktif ve tahmini teslim tarihi geçmiş
        public int DeliveredCargoCount { get; set; }
        public double DeliveryRatePercent { get; set; } // Teslim edilen / toplam (%)

        public int ActiveBranchCount { get; set; }
        public int EmployeeCount { get; set; }

        public List<DailyCountDto> Last7Days { get; set; } = new();
        public List<StatusCountDto> StatusDistribution { get; set; } = new();
        public List<BranchLoadDto> TopBranches { get; set; } = new();
    }
}