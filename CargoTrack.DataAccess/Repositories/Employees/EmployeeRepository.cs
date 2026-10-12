using CargoTrack.DataAccess.Context;
using CargoTrack.DataAccess.Repositories.GenericRepositories;
using CargoTrack.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace CargoTrack.DataAccess.Repositories.Employees
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // Neden: Manager'ın hangi şubeye bağlı olduğunu bulmak için.
        public async Task<Employee> GetByUserIdAsync(Guid userId)
        {
            return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        public async Task<(List<Employee> Items, int TotalCount)> GetPagedAsync(
            string? search, Guid? branchId, int page, int pageSize)
        {
            // Not: Silinmiş (soft delete) kayıtlar global query filter ile zaten gelmez.
            var query = _context.Employees.AsQueryable();

            if (branchId is not null)
            {
                query = query.Where(e => e.BranchId == branchId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.User.FirstName.Contains(search) ||
                    e.User.LastName.Contains(search) ||
                    (e.User.Email != null && e.User.Email.Contains(search)) ||
                    (e.User.UserName != null && e.User.UserName.Contains(search)) ||
                    e.Title.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.User.FirstName)
                .ThenBy(e => e.User.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}