using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    // Neden: Dağıtıma çıkınca 6 haneli kod üretilir, doğrulanmadan "Teslim Edildi" olunamaz.
    public class Delivery : BaseEntity
    {
        public Guid CargoId { get; set; }
        public string DeliveryCode { get; set; }
        public DateTime CodeCreatedDate { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string ReceiverName { get; set; }      // Teslim alan kişi
        public Guid? DeliveredByUserId { get; set; }  // Teslim eden personel

        public virtual Cargo Cargo { get; set; }
        public virtual AppUser DeliveredByUser { get; set; }
    }
}