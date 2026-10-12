namespace CargoTrack.DTO.DTOs.DashboardDtos
{
    // Neden: Manager dashboard'unun tüm rakamları tek nesnede taşınır (sadece kendi şubesine ait).
    public class ManagerDashboardDto
    {
        public string BranchName { get; set; } = string.Empty;

        public int TodayIncomingCount { get; set; }    // Bugün varış şubesine ulaşan
        public int TodayOutgoingCount { get; set; }    // Bugün çıkış şubesinden ayrılan
        public int OutForDeliveryCount { get; set; }   // Şu an dağıtımda olan
        public int DeliveredTodayCount { get; set; }   // Bugün teslim edilen
        public int OverdueCount { get; set; }          // Geciken
        public int ProblemCount { get; set; }          // Sorunlu (teslim edilemedi / iade / başarısız denemesi olan)

        public List<RecentMovementDto> RecentMovements { get; set; } = new();
    }
}