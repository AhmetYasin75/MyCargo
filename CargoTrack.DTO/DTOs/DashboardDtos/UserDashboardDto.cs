using CargoTrack.DTO.DTOs.Cargos;

namespace CargoTrack.DTO.DTOs.DashboardDtos
{
    // Neden: Müşteri dashboard'unun tüm verisi tek nesnede taşınır (sadece kullanıcının kendi kargoları).
    public class UserDashboardDto
    {
        public int ActiveCargoCount { get; set; }       // Yolda olanlar
        public int HistoryCargoCount { get; set; }      // Teslim edilmiş veya iade edilmiş

        public List<UserCargoSummaryDto> ActiveCargos { get; set; } = new();

        // Son oluşturulan kargo ve hareket zaman çizelgesi (kargo yoksa boş)
        public UserCargoSummaryDto? LastCargo { get; set; }
        public List<CargoMovementDto> LastCargoTimeline { get; set; } = new();
    }
}