using CargoTrack.Entity.Entities.Common;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.Entity.Entities
{
    // Neden: Fiyat kodun içine gömülmesin, Admin "fiyat kuralları" ekranından değiştirebilsin.
    public class CargoPrice : BaseEntity
    {
        public DeliveryType DeliveryType { get; set; }
        public PriceZone Zone { get; set; }
        public decimal BasePrice { get; set; }
        public decimal PricePerDesi { get; set; }
        public decimal PricePerKg { get; set; }
        public decimal InsuranceRate { get; set; }   // Ör: 0.02 = %2
        public bool IsActive { get; set; }
    }
}