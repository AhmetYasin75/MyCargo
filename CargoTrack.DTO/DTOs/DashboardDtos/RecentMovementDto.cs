using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.DashboardDtos
{
    // Neden: Manager dashboard'undaki "son hareketler" listesinin bir satırı.
    public class RecentMovementDto
    {
        public string TrackCode { get; set; } = string.Empty;
        public DateTime MovementDate { get; set; }
        public CargoStatus OldStatus { get; set; }
        public CargoStatus NewStatus { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? UserName { get; set; }
    }
}