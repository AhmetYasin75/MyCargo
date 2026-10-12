using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: Detay sayfasında kargonun tüm bilgileri ve hareket geçmişi (zaman çizelgesi) birlikte gösterilir.
    // Not: Teslimat kodu bilerek burada YOK. Kodu sadece alıcı görmeli (Faz 9, kullanıcı paneli).
    public class CargoDetailDto
    {
        public Guid Id { get; set; }
        public string TrackCode { get; set; }
        public DateTime ShipmentDate { get; set; }
        public DateTime EstimatedArrivalDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        public double Weight { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public double Height { get; set; }
        public double Desi { get; set; }
        public decimal Price { get; set; }
        public bool HasInsurance { get; set; }
        public int FailedDeliveryAttempts { get; set; }

        public CargoType CargoType { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public CargoStatus CargoStatus { get; set; }

        public string SenderFirstName { get; set; }
        public string SenderLastName { get; set; }
        public string ReceiverFirstName { get; set; }
        public string ReceiverLastName { get; set; }
        public string OriginBranchName { get; set; }
        public string DestinationBranchName { get; set; }

        // Neden: Süresi geçmiş ve henüz tamamlanmamış kargo "gecikmiş" sayılır (case SLA kuralı).
        public bool IsOverdue =>
            CargoStatus != CargoStatus.Delivered &&
            CargoStatus != CargoStatus.ReturnedToSender &&
            EstimatedArrivalDate < DateTime.Now;

        // Neden: Adı "Movements" olmasın, yoksa Mapster Cargo.Movements'tan (sıralamasız) otomatik doldurur.
        // Biz sıralı listeyi servis içinde kendimiz atıyoruz.
        public List<CargoMovementDto> Timeline { get; set; } = new();
    }
}