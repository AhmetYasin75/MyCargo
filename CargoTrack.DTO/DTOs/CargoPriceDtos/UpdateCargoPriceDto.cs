using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.CargoPriceDtos
{
    // Neden: Sayılar decimal? çünkü boş bırakılınca Türkçe mesajı FluentValidation versin (MVC'nin İngilizce hatası çıkmasın).
    // Teslimat tipi ve bölge sadece bilgi amaçlı gösterilir, güncellemede kullanılmaz (kural kimliği değişmez).
    public class UpdateCargoPriceDto
    {
        public Guid Id { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public PriceZone Zone { get; set; }

        public decimal? BasePrice { get; set; }
        public decimal? PricePerDesi { get; set; }
        public decimal? PricePerKg { get; set; }
        public decimal? InsuranceRatePercent { get; set; }   // Yüzde: 2 yazılır, 0.02 saklanır
    }
}