using CargoTrack.DataAccess.Repositories.Employees;
using CargoTrack.Entity.Entities;
using CargoTrack.Entity.Entities.Common;
using System.Linq.Expressions;

namespace CargoTrack.Business.Services.Security
{
    // Neden: "Kim neyi görür / neyi değiştirir" kuralları tek yerde. Kural değişirse sadece burası değişir.
    public class CargoAccessService(
        IUserContext _userContext,
        IEmployeeRepository _employeeRepository) : ICargoAccessService
    {
        // Giriş yapmamış kullanıcı için hata fırlatır
        public Guid CurrentUserId =>
            _userContext.UserId ?? throw new UnauthorizedAccessException("Giriş yapmanız gerekiyor.");

        // Liste sorgularına eklenecek filtre. Veritabanı sorgusunda uygulanır.
        public async Task<Expression<Func<Cargo, bool>>> GetVisibilityFilterAsync()
        {
            var userId = CurrentUserId;

            if (_userContext.IsInRole(RoleNames.Admin))
            {
                return c => true;   // Admin her şeyi görür
            }

            if (_userContext.IsInRole(RoleNames.Manager))
            {
                // Manager: kendi şubesine gelen VE kendi şubesinden çıkan kargolar
                var branchId = await GetManagerBranchIdAsync(userId);
                return c => c.OriginBranchId == branchId || c.DestinationBranchId == branchId;
            }

            if (_userContext.IsInRole(RoleNames.User))
            {
                // User: kendi gönderdiği ve kendisine gelen kargolar
                return c => c.SenderId == userId || c.ReceiverId == userId;
            }

            throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok.");
        }

        // Neden: Liste filtresiyle aynı kuralı tek kargo için de kullanıyoruz (kural iki yerde yazılmasın).
        // URL'deki Id değiştirilerek başkasının kargosuna erişilemez.
        public async Task EnsureCanViewAsync(Cargo cargo)
        {
            var filter = await GetVisibilityFilterAsync();
            if (!filter.Compile()(cargo))
            {
                throw new UnauthorizedAccessException("Bu kargoyu görüntüleme yetkiniz yok.");
            }
        }

        // Neden: Güncelleme, durum değiştirme, teslimat: Admin veya kargonun şubesindeki Manager yapabilir.
        // User sadece okur. Kural burada tek yerde; hem buton göstermek hem işlemi engellemek için kullanılır.
        public async Task<bool> CanOperateAsync(Cargo cargo)
        {
            var userId = CurrentUserId;

            if (_userContext.IsInRole(RoleNames.Admin))
            {
                return true;
            }

            if (_userContext.IsInRole(RoleNames.Manager))
            {
                var branchId = await GetManagerBranchIdAsync(userId);
                return cargo.OriginBranchId == branchId || cargo.DestinationBranchId == branchId;
            }

            return false;
        }

        // Neden: Yetki yoksa hata fırlatır. Kural CanOperateAsync'te, burada tekrar yazılmaz.
        public async Task EnsureCanOperateAsync(Cargo cargo)
        {
            if (!await CanOperateAsync(cargo))
            {
                throw new UnauthorizedAccessException("Bu kargo üzerinde işlem yapma yetkiniz yok.");
            }
        }

        // Manager sadece kendi şubesinden kargo oluşturabilir.
        public async Task EnsureCanCreateAsync(Guid originBranchId)
        {
            var userId = CurrentUserId;

            if (_userContext.IsInRole(RoleNames.Admin))
            {
                return;
            }

            if (_userContext.IsInRole(RoleNames.Manager))
            {
                var branchId = await GetManagerBranchIdAsync(userId);
                if (originBranchId == branchId)
                {
                    return;
                }

                throw new UnauthorizedAccessException("Sadece kendi şubenizden kargo oluşturabilirsiniz.");
            }

            throw new UnauthorizedAccessException("Kargo oluşturma yetkiniz yok.");
        }

        public void EnsureAdmin()
        {
            _ = CurrentUserId;

            if (!_userContext.IsInRole(RoleNames.Admin))
            {
                throw new UnauthorizedAccessException("Bu işlem sadece yöneticiye açıktır.");
            }
        }

        // Neden: Manager'ın şubesi Employee tablosunda tutuluyor. Kayıt yoksa şubeye bağlı değil demektir.
        private async Task<Guid> GetManagerBranchIdAsync(Guid userId)
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            if (employee is null)
            {
                throw new UnauthorizedAccessException("Herhangi bir şubeye bağlı değilsiniz.");
            }

            return employee.BranchId;
        }
    }
}