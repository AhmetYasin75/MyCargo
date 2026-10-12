using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: Durum değiştirmek için sadece bu bilgiler gerekir. Şubeyi servis kendisi belirler.
    public class ChangeCargoStatusDto
    {
        public Guid CargoId { get; set; }
        public CargoStatus NewStatus { get; set; }
        public Guid? TransferCenterId { get; set; }   // Sadece "Transfer Merkezinde" için zorunlu
        public string Description { get; set; }
        public Guid PerformedByUserId { get; set; }   // Controller giriş yapan kullanıcıdan doldurur
    }
}