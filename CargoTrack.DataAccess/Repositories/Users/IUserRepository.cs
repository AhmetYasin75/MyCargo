using CargoTrack.Entity.Entities;

namespace CargoTrack.DataAccess.Repositories.Users
{
    // Neden: AppUser, BaseEntity'den türemediği için IRepository<T> kullanamıyoruz. Küçük özel arayüz.
    public interface IUserRepository
    {
        Task<List<AppUser>> GetUsersInRoleAsync(string roleName);
    }
}