using CargoTrack.DataAccess.Context;
using CargoTrack.Entity.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CargoTrack.DataAccess.Repositories.AuditLogs
{
    public class AuditLogRepository(AppDbContext _context) : IAuditLogRepository
    {
        public async Task<(List<AuditLogRow> Items, int TotalCount)> GetPagedAsync(
            string? search, AuditActionType? actionType, string? entityName,
            DateTime? dateFrom, DateTime? dateToExclusive, int page, int pageSize)
        {
            // Neden: Left join. İşlemi yapan kullanıcı yoksa (UserId boş) kayıt yine de listelenir.
            var query =
                from l in _context.AuditLogs
                join u in _context.Users on l.UserId equals u.Id into users
                from u in users.DefaultIfEmpty()
                select new { Log = l, UserName = u != null ? u.UserName : null };

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.Log.Description.Contains(search) ||
                    x.Log.EntityName.Contains(search) ||
                    (x.UserName != null && x.UserName.Contains(search)));
            }

            if (actionType is not null)
            {
                query = query.Where(x => x.Log.ActionType == actionType);
            }

            if (!string.IsNullOrWhiteSpace(entityName))
            {
                query = query.Where(x => x.Log.EntityName == entityName);
            }

            if (dateFrom is not null)
            {
                query = query.Where(x => x.Log.CreatedDate >= dateFrom);
            }

            if (dateToExclusive is not null)
            {
                query = query.Where(x => x.Log.CreatedDate < dateToExclusive);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Log.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items.Select(x => new AuditLogRow(x.Log, x.UserName)).ToList(), totalCount);
        }

        public async Task<List<string>> GetEntityNamesAsync()
        {
            return await _context.AuditLogs
                .Select(l => l.EntityName)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();
        }
    }
}