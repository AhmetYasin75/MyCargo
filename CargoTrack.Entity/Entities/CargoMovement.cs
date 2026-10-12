using CargoTrack.Entity.Entities.Common;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.Entity.Entities
{
    // Neden: Her durum değişikliği silinmeyen bir kayıt olarak tutulur (kargo geçmişi / zaman çizelgesi).
    public class CargoMovement : BaseEntity
    {
        public Guid CargoId { get; set; }
        public CargoStatus OldStatus { get; set; }
        public CargoStatus NewStatus { get; set; }
        public DateTime MovementDate { get; set; }
        public string Description { get; set; }

        public Guid? BranchId { get; set; }           // Hareket şubede olduysa dolu
        public Guid? TransferCenterId { get; set; }   // Transfer merkezinde olduysa dolu
        public Guid? PerformedByUserId { get; set; }  // İşlemi yapan kişi

        public virtual Cargo Cargo { get; set; }
        public virtual Branch Branch { get; set; }
        public virtual TransferCenter TransferCenter { get; set; }
        public virtual AppUser PerformedByUser { get; set; }
    }
}