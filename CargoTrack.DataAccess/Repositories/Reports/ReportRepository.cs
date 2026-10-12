using CargoTrack.DataAccess.Context;
using CargoTrack.Entity.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CargoTrack.DataAccess.Repositories.Reports
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BranchReportRow>> GetBranchReportAsync(DateTime? from, DateTime? toExclusive)
        {
            var now = DateTime.Now;

            // Neden: Aralık filtresi gönderim tarihine göre. Soft delete filtresi otomatik uygulanır.
            var cargos = _context.Cargos.AsQueryable();
            if (from.HasValue) cargos = cargos.Where(c => c.ShipmentDate >= from.Value);
            if (toExclusive.HasValue) cargos = cargos.Where(c => c.ShipmentDate < toExclusive.Value);

            // Giden taraf: çıkış şubesine göre
            var sent = await cargos
                .GroupBy(c => c.OriginBranchId)
                .Select(g => new
                {
                    BranchId = g.Key,
                    Sent = g.Count(),
                    Returned = g.Count(c => c.CargoStatus == CargoStatus.ReturnedToSender)
                })
                .ToListAsync();

            // Gelen taraf: varış şubesine göre (teslim performansı varış şubesinin işi)
            var received = await cargos
                .GroupBy(c => c.DestinationBranchId)
                .Select(g => new
                {
                    BranchId = g.Key,
                    Received = g.Count(),
                    Delivered = g.Count(c => c.CargoStatus == CargoStatus.Delivered),
                    OnTime = g.Count(c => c.CargoStatus == CargoStatus.Delivered
                                          && c.DeliveredDate <= c.EstimatedArrivalDate),
                    Delayed = g.Count(c =>
                        (c.CargoStatus == CargoStatus.Delivered && c.DeliveredDate > c.EstimatedArrivalDate)
                        || (c.CargoStatus != CargoStatus.Delivered
                            && c.CargoStatus != CargoStatus.ReturnedToSender
                            && c.EstimatedArrivalDate < now)),
                    Minutes = g.Sum(c => c.CargoStatus == CargoStatus.Delivered && c.DeliveredDate != null
                        ? EF.Functions.DateDiffMinute(c.ShipmentDate, c.DeliveredDate.Value)
                        : 0)
                })
                .ToListAsync();

            // Neden: Hiç kargosu olmayan şube de raporda 0'larla görünsün.
            var branches = await _context.Branches
                .Select(b => new { b.Id, b.Name })
                .ToListAsync();

            return branches.Select(b =>
            {
                var s = sent.FirstOrDefault(x => x.BranchId == b.Id);
                var r = received.FirstOrDefault(x => x.BranchId == b.Id);
                return new BranchReportRow
                {
                    BranchId = b.Id,
                    BranchName = b.Name,
                    SentCount = s?.Sent ?? 0,
                    ReturnedCount = s?.Returned ?? 0,
                    ReceivedCount = r?.Received ?? 0,
                    DeliveredCount = r?.Delivered ?? 0,
                    OnTimeCount = r?.OnTime ?? 0,
                    DelayedCount = r?.Delayed ?? 0,
                    DeliveryMinutesSum = r?.Minutes ?? 0
                };
            }).ToList();
        }

        public async Task<List<EmployeeReportRow>> GetEmployeeReportAsync(DateTime? from, DateTime? toExclusive)
        {
            var today = DateTime.Today;

            var deliveries = _context.Deliveries.Where(d => d.IsVerified && d.DeliveredByUserId != null);
            if (from.HasValue) deliveries = deliveries.Where(d => d.VerifiedDate >= from.Value);
            if (toExclusive.HasValue) deliveries = deliveries.Where(d => d.VerifiedDate < toExclusive.Value);

            var failures = _context.DeliveryExceptions.Where(e => e.ReportedByUserId != null);
            if (from.HasValue) failures = failures.Where(e => e.OccurredDate >= from.Value);
            if (toExclusive.HasValue) failures = failures.Where(e => e.OccurredDate < toExclusive.Value);

            var movements = _context.CargoMovements.Where(m => m.PerformedByUserId != null);
            if (from.HasValue) movements = movements.Where(m => m.MovementDate >= from.Value);
            if (toExclusive.HasValue) movements = movements.Where(m => m.MovementDate < toExclusive.Value);

            var deliveredRows = await deliveries
                .GroupBy(d => d.DeliveredByUserId!.Value)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var failedRows = await failures
                .GroupBy(e => e.ReportedByUserId!.Value)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var opRows = await movements
                .GroupBy(m => m.PerformedByUserId!.Value)
                .Select(g => new
                {
                    UserId = g.Key,
                    Total = g.Count(),
                    Today = g.Count(m => m.MovementDate >= today)
                })
                .ToListAsync();

            // Neden: Önce hareket gören kullanıcıların kimliklerini topla, adları tek sorguyla çek.
            var userIds = deliveredRows.Select(x => x.UserId)
                .Union(failedRows.Select(x => x.UserId))
                .Union(opRows.Select(x => x.UserId))
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToListAsync();

            var employees = await _context.Employees
                .Where(e => userIds.Contains(e.UserId))
                .Select(e => new { e.UserId, BranchName = e.Branch.Name })
                .ToListAsync();

            return users.Select(u => new EmployeeReportRow
            {
                UserId = u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                BranchName = employees.FirstOrDefault(e => e.UserId == u.Id)?.BranchName ?? "-",
                DeliveredCount = deliveredRows.FirstOrDefault(x => x.UserId == u.Id)?.Count ?? 0,
                FailedCount = failedRows.FirstOrDefault(x => x.UserId == u.Id)?.Count ?? 0,
                TotalOperations = opRows.FirstOrDefault(x => x.UserId == u.Id)?.Total ?? 0,
                TodayOperations = opRows.FirstOrDefault(x => x.UserId == u.Id)?.Today ?? 0
            })
            .OrderByDescending(r => r.TotalOperations)
            .ToList();
        }
    }
}