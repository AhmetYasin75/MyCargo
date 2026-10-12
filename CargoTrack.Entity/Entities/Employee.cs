using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Entity.Entities
{
    // Neden: Manager'ın hangi şubeye bağlı olduğunu bilmeden "sadece kendi şubesini görsün" kuralı yazılamaz.
    public class Employee : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid BranchId { get; set; }
        public string Title { get; set; }   // Ör: Kurye, Şube Müdürü

        public virtual AppUser User { get; set; }
        public virtual Branch Branch { get; set; }
    }
}