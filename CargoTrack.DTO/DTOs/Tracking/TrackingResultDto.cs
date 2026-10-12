using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Tracking
{
    // Neden: Giriş yapmadan görülen sonuç. Gönderici/alıcı adı, teslimat kodu ve fiyat yok.
    public class TrackingResultDto
    {
        public string TrackCode { get; set; } = string.Empty;
        public CargoStatus Status { get; set; }
        public DateTime ShipmentDate { get; set; }
        public DateTime EstimatedArrivalDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public bool IsOverdue { get; set; }

        public string OriginCity { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public string OriginBranch { get; set; } = string.Empty;
        public string DestinationBranch { get; set; } = string.Empty;

        public CargoType CargoType { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public double Weight { get; set; }
        public double Desi { get; set; }

        public int ProgressPercent { get; set; }
        public string? LastLocation { get; set; }

        public List<TrackingMovementDto> Movements { get; set; } = new();
    }
}