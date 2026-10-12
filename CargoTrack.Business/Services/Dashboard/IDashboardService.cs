using CargoTrack.DTO.DTOs.DashboardDtos;

namespace CargoTrack.Business.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto> GetAdminDashboardAsync();
        Task<ManagerDashboardDto> GetManagerDashboardAsync();
        Task<UserDashboardDto> GetUserDashboardAsync();
    }
}