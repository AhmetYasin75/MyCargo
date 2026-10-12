using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: Kullanıcıdan sadece girmesi gereken alanlar alınır. TrackCode, durum, fiyat gibi
    // alanları kullanıcı belirlememeli, bunları servis üretecek.
    public class CreateCargoDto
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid OriginBranchId { get; set; }
        public Guid DestinationBranchId { get; set; }
        public double Weight { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public double Height { get; set; }
        public CargoType CargoType { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public bool HasInsurance { get; set; }

        public Guid? PerformedByUserId { get; set; }   // Neden: İlk hareket kaydında işlemi yapan görünsün
    }
}