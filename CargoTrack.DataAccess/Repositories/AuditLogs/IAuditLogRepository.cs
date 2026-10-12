using CargoTrack.Entity.Entities.Enums;

namespace CargoTrack.DataAccess.Repositories.AuditLogs
{
    // Neden: Audit kayıtları sadece okunur (AppDbContext otomatik üretir), bu yüzden IRepository<T> yok.
    public interface IAuditLogRepository
    {
        Task<(List<AuditLogRow> Items, int TotalCount)> GetPagedAsync(
            string? search, AuditActionType? actionType, string? entityName,
            DateTime? dateFrom, DateTime? dateToExclusive, int page, int pageSize);

        // Neden: Filtre kutusundaki "Varlık" listesi, kayıtlarda geçen isimlerden dolar.
        Task<List<string>> GetEntityNamesAsync();
    }
}