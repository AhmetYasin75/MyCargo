using CargoTrack.DataAccess.Repositories.GenericRepositories;
using CargoTrack.Entity.Entities;

namespace CargoTrack.DataAccess.Repositories.Employees
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<Employee> GetByUserIdAsync(Guid userId);

        // Neden: Personel listesi sayfalı, aramalı ve şubeye göre filtrelenebilir. İş veritabanında yapılır.
        Task<(List<Employee> Items, int TotalCount)> GetPagedAsync(string? search, Guid? branchId, int page, int pageSize);
    }
}