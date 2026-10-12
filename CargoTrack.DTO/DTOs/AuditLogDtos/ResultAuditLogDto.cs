using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.AuditLogDtos
{
    public class ResultAuditLogDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UserName { get; set; }       // Boşsa işlemi giriş yapmış kullanıcı yapmamıştır
        public AuditActionType ActionType { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}