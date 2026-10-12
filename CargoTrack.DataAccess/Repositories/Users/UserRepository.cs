using CargoTrack.DataAccess.Context;
using CargoTrack.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace CargoTrack.DataAccess.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // Neden: Belirli roldeki kullanıcıları getirir (örn. müşteri = "User" rolündekiler).
        public async Task<List<AppUser>> GetUsersInRoleAsync(string roleName)
        {
            return await (from user in _context.Users
                          join userRole in _context.UserRoles on user.Id equals userRole.UserId
                          join role in _context.Roles on userRole.RoleId equals role.Id
                          where role.Name == roleName
                          orderby user.FirstName, user.LastName
                          select user).ToListAsync();
        }
    }
}