using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: Listede Guid yerine isimler görünsün. Mapster, "OriginBranchName" gibi isimleri
    // OriginBranch.Name'e bakarak otomatik doldurur (flattening), ayrı ayar gerekmez.
    public class ResultCargoDto
    {
        public Guid Id { get; set; }
        public string TrackCode { get; set; }
        public DateTime ShipmentDate { get; set; }
        public DateTime EstimatedArrivalDate { get; set; }
        public double Weight { get; set; }
        public double Desi { get; set; }
        public decimal Price { get; set; }
        public CargoType CargoType { get; set; }
        public CargoStatus CargoStatus { get; set; }
        public string SenderFirstName { get; set; }
        public string SenderLastName { get; set; }
        public string ReceiverFirstName { get; set; }
        public string ReceiverLastName { get; set; }
        public string OriginBranchName { get; set; }
        public string DestinationBranchName { get; set; }

        // Neden: Süresi geçmiş ve henüz tamamlanmamış kargo "gecikmiş" sayılır.
        public bool IsOverdue =>
            CargoStatus != CargoStatus.Delivered &&
            CargoStatus != CargoStatus.ReturnedToSender &&
            EstimatedArrivalDate < DateTime.Now;
    }
}