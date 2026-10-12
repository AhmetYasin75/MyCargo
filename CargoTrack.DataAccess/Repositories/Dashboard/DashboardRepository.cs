using CargoTrack.DataAccess.Context;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CargoTrack.DataAccess.Repositories.Dashboard
{
    public class DashboardRepository(AppDbContext _context) : IDashboardRepository
    {
        // Not: Silinmiş (soft delete) kayıtlar global query filter ile zaten dışarıda kalır.
        public async Task<int> CountCargosAsync(Expression<Func<Cargo, bool>> filter)
        {
            return await _context.Cargos.Where(filter).CountAsync();
        }

        public async Task<Dictionary<CargoStatus, int>> GetStatusCountsAsync(Expression<Func<Cargo, bool>> filter)
        {
            return await _context.Cargos
                .Where(filter)
                .GroupBy(c => c.CargoStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<Dictionary<DateTime, int>> GetDailyShipmentCountsAsync(
            Expression<Func<Cargo, bool>> filter, DateTime fromDate)
        {
            return await _context.Cargos
                .Where(filter)
                .Where(c => c.ShipmentDate >= fromDate)
                .GroupBy(c => c.ShipmentDate.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Day, x => x.Count);
        }

        public async Task<List<(string BranchName, int CargoCount)>> GetTopBranchesAsync(int take)
        {
            var rows = await _context.Branches
                .Select(b => new
                {
                    b.Name,
                    Count = _context.Cargos.Count(c => c.OriginBranchId == b.Id || c.DestinationBranchId == b.Id)
                })
                .Where(x => x.Count > 0)
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name)
                .Take(take)
                .ToListAsync();

            return rows.Select(r => (r.Name, r.Count)).ToList();
        }

        public async Task<int> CountBranchesAsync()
        {
            return await _context.Branches.CountAsync();
        }

        public async Task<int> CountEmployeesAsync()
        {
            return await _context.Employees.CountAsync();
        }

        public async Task<int> CountMovementCargosAsync(Expression<Func<CargoMovement, bool>> movementFilter)
        {
            return await _context.CargoMovements
                .Where(movementFilter)
                .Select(m => m.CargoId)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<RecentMovementRow>> GetRecentMovementsAsync(
            Expression<Func<Cargo, bool>> cargoFilter, int take)
        {
            // Neden: Hareketi, kargosu ve işlemi yapan kullanıcıyla birleştiriyoruz (kullanıcı yoksa kayıt yine gelir).
            var rows = await (
                from m in _context.CargoMovements
                join c in _context.Cargos.Where(cargoFilter) on m.CargoId equals c.Id
                join u in _context.Users on m.PerformedByUserId equals u.Id into users
                from u in users.DefaultIfEmpty()
                orderby m.MovementDate descending
                select new
                {
                    c.TrackCode,
                    m.MovementDate,
                    m.OldStatus,
                    m.NewStatus,
                    m.Description,
                    UserName = u != null ? u.UserName : null
                })
                .Take(take)
                .ToListAsync();

            return rows
                .Select(r => new RecentMovementRow(
                    r.TrackCode, r.MovementDate, r.OldStatus, r.NewStatus, r.Description, r.UserName))
                .ToList();
        }
    }
}