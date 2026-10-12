using CargoTrack.Business.Services.Security;
using CargoTrack.DataAccess.Repositories.Branches;
using CargoTrack.DataAccess.Repositories.Employees;
using CargoTrack.DTO.DTOs.Common;
using CargoTrack.DTO.DTOs.EmployeeDtos;
using CargoTrack.Entity.Entities;
using Mapster;
using System.ComponentModel.DataAnnotations;

namespace CargoTrack.Business.Services.Employees
{
    public class EmployeeService(
        IEmployeeRepository _employeeRepository,
        IBranchRepository _branchRepository,
        ICargoAccessService _accessService) : IEmployeeService
    {
        // Neden: URL'den pageSize=100000 yazılıp sistemin yorulmasını önler.
        private const int MaxPageSize = 50;

        // Neden: Rol atama ekranında kullanıcının mevcut şubesini önceden seçili göstermek için.
        public async Task<Guid?> GetBranchIdAsync(Guid userId)
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            return employee?.BranchId;
        }

        // Neden: Manager'ı şubeye bağlar. Kayıt varsa şubesini günceller, yoksa yeni oluşturur.
        // Böylece bir kullanıcının tek Employee kaydı olur.
        public async Task AssignToBranchAsync(Guid userId, Guid branchId)
        {
            var branch = await _branchRepository.GetByIdAsync(branchId);
            if (branch is null)
            {
                throw new ValidationException("Branch Not Found");
            }

            var employee = await _employeeRepository.GetByUserIdAsync(userId);

            if (employee is null)
            {
                await _employeeRepository.CreateAsync(new Employee
                {
                    UserId = userId,
                    BranchId = branchId,
                    Title = "Şube Yöneticisi"
                });
            }
            else
            {
                employee.BranchId = branchId;
                await _employeeRepository.UpdateAsync(employee);
            }
        }

        // Neden: Manager rolü kaldırılınca şube bağı da kalkar (soft delete). Kayıt yoksa bir şey yapmaz.
        public async Task RemoveFromBranchAsync(Guid userId)
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            if (employee is not null)
            {
                await _employeeRepository.DeleteAsync(employee);
            }
        }

        // ---------------- FAZ 8D: ADMİN PERSONEL LİSTESİ ----------------

        public async Task<PagedResult<ResultEmployeeDto>> GetPagedAsync(EmployeeFilterDto filter)
        {
            _accessService.EnsureAdmin();

            filter ??= new EmployeeFilterDto();
            int page = Math.Max(filter.Page, 1);
            int pageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);

            var (items, totalCount) = await _employeeRepository.GetPagedAsync(
                filter.Search?.Trim(), filter.BranchId, page, pageSize);

            return new PagedResult<ResultEmployeeDto>
            {
                Items = items.Adapt<List<ResultEmployeeDto>>(),
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UpdateEmployeeDto> GetByIdAsync(Guid employeeId)
        {
            _accessService.EnsureAdmin();

            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee is null)
            {
                throw new ValidationException("Personel bulunamadı.");
            }

            return employee.Adapt<UpdateEmployeeDto>();
        }

        public async Task UpdateAsync(UpdateEmployeeDto dto)
        {
            _accessService.EnsureAdmin();

            // Neden: Yeni nesne üretmiyoruz, mevcut kaydı çekip sadece ünvan ve şubeyi değiştiriyoruz.
            var employee = await _employeeRepository.GetByIdAsync(dto.Id);
            if (employee is null)
            {
                throw new ValidationException("Personel bulunamadı.");
            }

            var branch = await _branchRepository.GetByIdAsync(dto.BranchId);
            if (branch is null)
            {
                throw new ValidationException("Şube bulunamadı.");
            }

            employee.Title = dto.Title!.Trim();
            employee.BranchId = dto.BranchId;

            await _employeeRepository.UpdateAsync(employee);
        }
    }
}