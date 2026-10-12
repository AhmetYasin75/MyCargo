using CargoTrack.DataAccess.Repositories.GenericRepositories;
using CargoTrack.Entity.Entities;

namespace CargoTrack.DataAccess.Repositories.TransferCenters
{
    public interface ITransferCenterRepository : IRepository<TransferCenter>
    {
        // Neden: Büyük listeler tek seferde yüklenmesin. Sayfalama ve arama veritabanında yapılır.
        Task<(List<TransferCenter> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize);

        // Neden: Hareket geçmişinde kullanılan merkez silinemez.
        Task<bool> HasMovementsAsync(Guid id);

        // Neden: Aynı isimde ikinci merkez eklenmesin. Güncellemede kendi kaydı hariç tutulur.
        Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    }
}