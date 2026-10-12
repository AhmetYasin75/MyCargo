using CargoTrack.DTO.DTOs.Common;
using CargoTrack.DTO.DTOs.EmployeeDtos;

namespace CargoTrack.Business.Services.Employees
{
    public interface IEmployeeService
    {
        // Rol atama ekranı için (mevcut)
        Task<Guid?> GetBranchIdAsync(Guid userId);
        Task AssignToBranchAsync(Guid userId, Guid branchId);
        Task RemoveFromBranchAsync(Guid userId);

        // Faz 8D: Admin personel listesi
        Task<PagedResult<ResultEmployeeDto>> GetPagedAsync(EmployeeFilterDto filter);
        Task<UpdateEmployeeDto> GetByIdAsync(Guid employeeId);
        Task UpdateAsync(UpdateEmployeeDto dto);
    }
}