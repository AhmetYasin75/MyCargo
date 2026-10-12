using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.Cargos
{
    // Neden: Zaman çizelgesinde şube/merkez/personel isimleri görünsün.
    // Mapster "BranchName" alanını Branch.Name'den kendisi doldurur.
    public class CargoMovementDto
    {
        public Guid Id { get; set; }
        public CargoStatus OldStatus { get; set; }
        public CargoStatus NewStatus { get; set; }
        public DateTime MovementDate { get; set; }
        public string Description { get; set; }
        public string BranchName { get; set; }
        public string TransferCenterName { get; set; }
        public string PerformedByUserFirstName { get; set; }
        public string PerformedByUserLastName { get; set; }
    }
}