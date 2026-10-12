using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DTO.DTOs.AuditLogDtos
{
    // Neden: Arama, filtre ve sayfalama değerleri URL'den bu nesneye dolar.
    public class AuditLogFilterDto
    {
        public string? Search { get; set; }
        public AuditActionType? ActionType { get; set; }
        public string? EntityName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}