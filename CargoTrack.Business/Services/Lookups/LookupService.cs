using CargoTrack.Business.Services.Employees;
using CargoTrack.Business.Services.Security;
using CargoTrack.DataAccess.Repositories.Branches;
using CargoTrack.DataAccess.Repositories.TransferCenters;
using CargoTrack.DataAccess.Repositories.Users;
using CargoTrack.DTO.DTOs.Common;
using CargoTrack.Entity.Entities.Common;

namespace CargoTrack.Business.Services.Lookups
{
    public class LookupService(
        IUserRepository _userRepository,
        IBranchRepository _branchRepository,
        ITransferCenterRepository _transferCenterRepository,
        IUserContext _userContext,
        IEmployeeService _employeeService) : ILookupService
    {
        // Müşteri = "User" rolündeki kullanıcı (gönderici/alıcı seçimi için)
        public async Task<List<LookupItemDto>> GetCustomersAsync()
        {
            var users = await _userRepository.GetUsersInRoleAsync(RoleNames.User);

            return users.Select(u => new LookupItemDto
            {
                Id = u.Id,
                Text = $"{u.FirstName} {u.LastName} ({u.UserName})"
            }).ToList();
        }

        public async Task<List<LookupItemDto>> GetBranchesAsync()
        {
            var branches = await _branchRepository.GetAllAsync();

            return branches.OrderBy(b => b.Name).Select(b => new LookupItemDto
            {
                Id = b.Id,
                Text = b.Name
            }).ToList();
        }

        // Neden: Admin tüm şubelerden kargo oluşturabilir, Manager sadece kendi şubesinden.
        // Bu liste sadece kolaylık için; asıl kural CargoService.CreateAsync içinde kontrol ediliyor.
        public async Task<List<LookupItemDto>> GetOriginBranchesAsync()
        {
            var allBranches = await GetBranchesAsync();

            if (_userContext.IsInRole(RoleNames.Admin))
            {
                return allBranches;
            }

            if (_userContext.IsInRole(RoleNames.Manager))
            {
                var userId = _userContext.UserId
                    ?? throw new UnauthorizedAccessException("Giriş yapmanız gerekiyor.");

                var branchId = await _employeeService.GetBranchIdAsync(userId);
                if (branchId is null)
                {
                    throw new UnauthorizedAccessException("Herhangi bir şubeye bağlı değilsiniz.");
                }

                return allBranches.Where(b => b.Id == branchId.Value).ToList();
            }

            throw new UnauthorizedAccessException("Kargo oluşturma yetkiniz yok.");
        }

        // Neden: Durum değiştir formunda "Transfer Merkezinde" seçilince hangi merkez olduğu sorulur.
        public async Task<List<LookupItemDto>> GetTransferCentersAsync()
        {
            var centers = await _transferCenterRepository.GetAllAsync();

            return centers.OrderBy(c => c.Name).Select(c => new LookupItemDto
            {
                Id = c.Id,
                Text = c.Name
            }).ToList();
        }
    }
}