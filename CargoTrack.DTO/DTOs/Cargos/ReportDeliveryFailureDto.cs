using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: Başarısız teslimatta neden (adres hatalı, alıcı yok...) ve açıklama kaydedilir.
    public class ReportDeliveryFailureDto
    {
        public Guid CargoId { get; set; }
        public DeliveryExceptionReason Reason { get; set; }
        public string Description { get; set; }
        public Guid PerformedByUserId { get; set; }
    }
}