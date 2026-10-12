using CargoTrack.DataAccess.Context;
using CargoTrack.DataAccess.Repositories.GenericRepositories;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace CargoTrack.DataAccess.Repositories.CargoPrices
{
    public class CargoPriceRepository : GenericRepository<CargoPrice>, ICargoPriceRepository
    {
        private readonly AppDbContext _context;

        public CargoPriceRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // Neden: Teslimat tipi ve bölge için şu an geçerli (aktif) fiyat kuralını getirir.
        public async Task<CargoPrice> GetActivePriceAsync(DeliveryType deliveryType, PriceZone zone)
        {
            return await _context.CargoPrices
                .FirstOrDefaultAsync(p => p.DeliveryType == deliveryType && p.Zone == zone && p.IsActive);
        }
    }
}