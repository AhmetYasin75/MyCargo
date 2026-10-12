using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.DashboardDtos
{
    // Neden: "Durum dağılımı" grafiğinin bir dilimi.
    public class StatusCountDto
    {
        public CargoStatus Status { get; set; }
        public int Count { get; set; }
    }
}