using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Enums;
using System.Linq.Expressions;

namespace CargoTrack.DataAccess.Repositories.Dashboard
{
    // Neden: Dashboard rakamları (sayı, gruplama) veritabanında hesaplanır, kargolar belleğe çekilmez.
    // Metotlar filtre alır: Admin "hepsi", Manager "kendi şubesi", User "kendi kargosu" filtresini verir.
    public interface IDashboardRepository
    {
        Task<int> CountCargosAsync(Expression<Func<Cargo, bool>> filter);

        Task<Dictionary<CargoStatus, int>> GetStatusCountsAsync(Expression<Func<Cargo, bool>> filter);

        // Gönderim tarihine göre günlük kargo sayısı (anahtar: günün başlangıcı)
        Task<Dictionary<DateTime, int>> GetDailyShipmentCountsAsync(Expression<Func<Cargo, bool>> filter, DateTime fromDate);

        // Çıkış veya varış şubesi o şube olan kargo sayısına göre en yoğun şubeler
        Task<List<(string BranchName, int CargoCount)>> GetTopBranchesAsync(int take);

        Task<int> CountBranchesAsync();
        Task<int> CountEmployeesAsync();

        // Faz 9 (Manager): Filtreye uyan hareketlerdeki FARKLI kargo sayısı (aynı kargo iki kez sayılmasın)
        Task<int> CountMovementCargosAsync(Expression<Func<CargoMovement, bool>> movementFilter);

        // Faz 9 (Manager): Filtreye uyan kargoların en son hareketleri
        Task<List<RecentMovementRow>> GetRecentMovementsAsync(Expression<Func<Cargo, bool>> cargoFilter, int take);
    }
}