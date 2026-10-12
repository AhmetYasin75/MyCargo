using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.DashboardDtos
{
    // Neden: Müşteri panelinde bir kargonun özeti. Başkasının kişisel bilgisi için sadece ad-soyad gösterilir.
    public class UserCargoSummaryDto
    {
        public Guid Id { get; set; }
        public string TrackCode { get; set; } = string.Empty;
        public CargoStatus CargoStatus { get; set; }
        public DateTime EstimatedArrivalDate { get; set; }
        public string OriginBranchName { get; set; } = string.Empty;
        public string DestinationBranchName { get; set; } = string.Empty;

        // Kullanıcı bu kargonun göndericisi mi, alıcısı mı?
        public bool IsSender { get; set; }

        // Karşı tarafın adı (göndericiysem alıcı, alıcıysam gönderici)
        public string OtherPartyName { get; set; } = string.Empty;

        // Neden: Teslimat kodunu sadece ALICI görür ve sadece kargo "Dağıtıma Çıktı" iken. Diğer durumlarda boş.
        public string? DeliveryCode { get; set; }

        // Neden: Süresi geçmiş ve henüz tamamlanmamış kargo "gecikmiş" sayılır (case SLA kuralı).
        public bool IsOverdue =>
            CargoStatus != CargoStatus.Delivered &&
            CargoStatus != CargoStatus.ReturnedToSender &&
            EstimatedArrivalDate < DateTime.Now;
    }
}