using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.CargoPriceDtos
{
    // Neden: Listede sigorta oranı yüzde olarak gösterilir (0.02 değil, 2).
    public class ResultCargoPriceDto
    {
        public Guid Id { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public PriceZone Zone { get; set; }
        public decimal BasePrice { get; set; }
        public decimal PricePerDesi { get; set; }
        public decimal PricePerKg { get; set; }
        public decimal InsuranceRatePercent { get; set; }
        public bool IsActive { get; set; }
    }
}