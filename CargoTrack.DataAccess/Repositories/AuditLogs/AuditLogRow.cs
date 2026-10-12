using CargoTrack.Entity.Entities;

namespace CargoTrack.DataAccess.Repositories.AuditLogs
{
    // Neden: AuditLog'da sadece UserId var. Listede kullanıcı adı da göstermek için repository
    // kaydı ve kullanıcı adını birlikte döner.
    public record AuditLogRow(AuditLog Log, string? UserName);
}