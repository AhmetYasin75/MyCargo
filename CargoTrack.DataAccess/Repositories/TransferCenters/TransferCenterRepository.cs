using CargoTrack.DataAccess.Context;
using CargoTrack.DataAccess.Repositories.GenericRepositories;
using CargoTrack.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace CargoTrack.DataAccess.Repositories.TransferCenters
{
    public class TransferCenterRepository : GenericRepository<TransferCenter>, ITransferCenterRepository
    {
        private readonly AppDbContext _context;

        public TransferCenterRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(List<TransferCenter> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize)
        {
            // Not: Silinmiş (soft delete) kayıtlar global query filter ile zaten gelmez.
            var query = _context.TransferCenters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Name.Contains(search) || t.City.Name.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> HasMovementsAsync(Guid id)
        {
            return await _context.CargoMovements.AnyAsync(m => m.TransferCenterId == id);
        }

        public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
        {
            return await _context.TransferCenters
                .AnyAsync(t => t.Name == name && (excludeId == null || t.Id != excludeId));
        }
    }
}