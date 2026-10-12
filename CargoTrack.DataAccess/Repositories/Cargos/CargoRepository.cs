using CargoTrack.DataAccess.Context;
using CargoTrack.DataAccess.Repositories.GenericRepositories;
using CargoTrack.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CargoTrack.DataAccess.Repositories.Cargos
{
    public class CargoRepository : GenericRepository<Cargo>, ICargoRepository
    {
        private readonly AppDbContext _context;

        public CargoRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // Neden: Include ile şube ve kullanıcı bilgilerini tek sorguda getiriyoruz.
        // Faz 6: filter parametresi ile kullanıcının yetkisi dahilindeki kargolar veritabanında filtrelenir
        // (hepsini çekip sonra süzmüyoruz, yoksa büyük listelerde yavaşlar ve gereksiz veri taşınır).
        public async Task<List<Cargo>> GetAllWithDetailsAsync(Expression<Func<Cargo, bool>> filter = null)
        {
            IQueryable<Cargo> query = _context.Cargos
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .Include(c => c.OriginBranch)
                .Include(c => c.DestinationBranch);

            if (filter is not null)
            {
                query = query.Where(filter);
            }

            return await query.OrderByDescending(c => c.ShipmentDate).ToListAsync();
        }

        // Faz 8: Sayfalama. Filtreler tek tek Where ile eklenir (hepsi AND ile birleşir),
        // sonra toplam kayıt sayılır ve sadece istenen sayfanın kayıtları çekilir.
        public async Task<(List<Cargo> Items, int TotalCount)> GetPagedAsync(
            IEnumerable<Expression<Func<Cargo, bool>>> filters, int page, int pageSize)
        {
            IQueryable<Cargo> query = _context.Cargos
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .Include(c => c.OriginBranch)
                .Include(c => c.DestinationBranch);

            foreach (var filter in filters)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();

            // Neden: ThenBy(Id) aynı tarihli kayıtlarda sıranın sabit kalmasını sağlar,
            // yoksa sayfalar arasında aynı kayıt iki kez görünebilir ya da atlanabilir.
            var items = await query
                .OrderByDescending(c => c.ShipmentDate)
                .ThenBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // Neden: Herkese açık takip ekranı (Faz 10) takip no ile arama yapacak.
        public async Task<Cargo> GetByTrackCodeAsync(string trackCode)
        {
            return await _context.Cargos
                .Include(c => c.OriginBranch)
                .Include(c => c.DestinationBranch)
                .FirstOrDefaultAsync(c => c.TrackCode == trackCode);
        }

        // Neden: Üretilen takip no daha önce kullanıldı mı kontrolü (benzersiz olmalı).
        public async Task<bool> TrackCodeExistsAsync(string trackCode)
        {
            return await _context.Cargos.AnyAsync(c => c.TrackCode == trackCode);
        }

        // Neden: Zaman çizelgesi için hareketler eskiden yeniye sıralı gelir.
        public async Task<List<CargoMovement>> GetMovementsByCargoIdAsync(Guid cargoId)
        {
            return await _context.CargoMovements
                .Include(m => m.Branch)
                .Include(m => m.TransferCenter)
                .Include(m => m.PerformedByUser)
                .Where(m => m.CargoId == cargoId)
                .OrderBy(m => m.MovementDate)
                .ToListAsync();
        }

        // Neden: Bir işlemde birden fazla kayıt değişebilir (kargo durumu, hareketler, teslimat, hata kaydı).
        // Hepsi TEK SaveChanges ile kaydedilir: ya hepsi kaydolur ya hiçbiri (tutarsızlık olmaz).
        // Kargo servise FindAsync ile geldiği için zaten EF tarafından takip ediliyor, Update çağırmaya gerek yok.
        public async Task SaveOperationAsync(
            Cargo cargo,
            List<CargoMovement> movements,
            Delivery delivery = null,
            DeliveryException deliveryException = null)
        {
            await _context.CargoMovements.AddRangeAsync(movements);

            // Delivery yeni oluşturulduysa eklenir, daha önce vardıysa zaten takipte (değişiklikleri kaydolur)
            if (delivery is not null && _context.Entry(delivery).State == EntityState.Detached)
            {
                await _context.Deliveries.AddAsync(delivery);
            }

            if (deliveryException is not null)
            {
                await _context.DeliveryExceptions.AddAsync(deliveryException);
            }

            await _context.SaveChangesAsync();
        }
    }
}