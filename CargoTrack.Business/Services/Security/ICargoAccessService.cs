using CargoTrack.Entity.Entities;
using System.Linq.Expressions;

namespace CargoTrack.Business.Services.Security
{
    public interface ICargoAccessService
    {
        Guid CurrentUserId { get; }
        Task<Expression<Func<Cargo, bool>>> GetVisibilityFilterAsync();   // Listeler için
        Task EnsureCanViewAsync(Cargo cargo);                              // Tek kargo görüntüleme
        Task<bool> CanOperateAsync(Cargo cargo);                           // Evet/hayır cevabı (buton göstermek için)
        Task EnsureCanOperateAsync(Cargo cargo);                           // Güncelleme, durum, teslimat (yetki yoksa hata)
        Task EnsureCanCreateAsync(Guid originBranchId);                    // Kargo oluşturma
        void EnsureAdmin();                                                // Sadece Admin işlemleri
    }
}