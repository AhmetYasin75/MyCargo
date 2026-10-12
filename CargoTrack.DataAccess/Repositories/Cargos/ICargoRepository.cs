using CargoTrack.DataAccess.Repositories.GenericRepositories;
using CargoTrack.Entity.Entities;
using System.Linq.Expressions;

namespace CargoTrack.DataAccess.Repositories.Cargos
{
    public interface ICargoRepository : IRepository<Cargo>
    {
        Task<List<Cargo>> GetAllWithDetailsAsync(Expression<Func<Cargo, bool>> filter = null);
        Task<Cargo> GetByTrackCodeAsync(string trackCode);
        Task<bool> TrackCodeExistsAsync(string trackCode);
        Task<List<CargoMovement>> GetMovementsByCargoIdAsync(Guid cargoId);

        // Faz 5: Durum değişikliği + hareketler + (varsa) teslimat ve hata kaydı tek seferde kaydedilir
        Task SaveOperationAsync(
            Cargo cargo,
            List<CargoMovement> movements,
            Delivery delivery = null,
            DeliveryException deliveryException = null);

        // Faz 8: Filtrelenmiş ve sayfalanmış liste (sayfalama veritabanında yapılır)
        Task<(List<Cargo> Items, int TotalCount)> GetPagedAsync(
            IEnumerable<Expression<Func<Cargo, bool>>> filters, int page, int pageSize);
    }
}