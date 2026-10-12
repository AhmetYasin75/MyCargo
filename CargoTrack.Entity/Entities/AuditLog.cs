using CargoTrack.Entity.Entities.Common;
using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.Entity.Entities
{
    // Neden: Kim, ne zaman, neyi, eski/yeni değeriyle değiştirdi sorusunun cevabı.
    // İşlem tarihi için BaseEntity.CreatedDate kullanılır.
    public class AuditLog : BaseEntity
    {
        public Guid? UserId { get; set; }
        public AuditActionType ActionType { get; set; }
        public string EntityName { get; set; }
        public Guid EntityId { get; set; }

        // Neden: Kayıt oluşturulurken eski değer, silinirken yeni değer yoktur. Bu yüzden boş olabilir (string?).
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public string Description { get; set; }
    }
}