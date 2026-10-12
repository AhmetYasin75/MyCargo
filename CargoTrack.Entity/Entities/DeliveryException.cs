using CargoTrack.Entity.Entities.Common;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.Entity.Entities
{
    // Neden: Başarısız teslimat gerekçesiyle kayıt altına alınır, iade kararı bu kayıtların sayısına bağlanır.
    public class DeliveryException : BaseEntity
    {
        public Guid CargoId { get; set; }
        public DeliveryExceptionReason Reason { get; set; }
        public string Description { get; set; }
        public DateTime OccurredDate { get; set; }
        public Guid? ReportedByUserId { get; set; }

        public virtual Cargo Cargo { get; set; }
        public virtual AppUser ReportedByUser { get; set; }
    }
}