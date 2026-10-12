using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: Güncellemede durum/takip no gibi alanlar YOK. Durum sadece iş kuralıyla (Faz 3) değişir.
    public class UpdateCargoDto
    {
        public Guid Id { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid DestinationBranchId { get; set; }
        public double Weight { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public double Height { get; set; }
        public CargoType CargoType { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public bool HasInsurance { get; set; }
    }
}