using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Tracking
{
    // Neden: Herkese açık zaman çizelgesi satırı. Açıklama ve personel adı bilerek yok (kişisel veri riski).
    public class TrackingMovementDto
    {
        public CargoStatus Status { get; set; }
        public DateTime MovementDate { get; set; }
        public string Location { get; set; } = string.Empty;   // Şube veya aktarma merkezi adı
    }
}