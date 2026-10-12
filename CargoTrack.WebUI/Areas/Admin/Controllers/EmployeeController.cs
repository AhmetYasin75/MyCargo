using CargoTrack.Business.Services.Employees;
using CargoTrack.Business.Services.Lookups;
using CargoTrack.DTO.DTOs.EmployeeDtos;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    // Neden: Personel kaydı Rol Atama ekranından oluşur (Manager + şube). Bu ekran listeleme,
    // arama, şubeye göre filtreleme ve ünvan/şube güncelleme içindir.
    [Area(Area.Admin)]
    [Authorize(Roles = Roles.Admin)]
    public class EmployeeController(
        IEmployeeService _service,
        ILookupService _lookupService) : Controller
    {
        public async Task<IActionResult> Index([FromQuery] EmployeeFilterDto filter)
        {
            var result = await _service.GetPagedAsync(filter);

            // Arama ve şube kutularının dolu kalması için
            ViewBag.Filter = filter;
            await LoadBranchesAsync(filter.BranchId);

            return View(result);
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var dto = await _service.GetByIdAsync(id);
            await LoadBranchesAsync(dto.BranchId);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadBranchesAsync(dto.BranchId);
                return View(dto);
            }

            await _service.UpdateAsync(dto);

            TempData["Success"] = "Personel bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Neden: Hem filtre kutusu hem güncelleme formu aynı şube listesini kullanır.
        private async Task LoadBranchesAsync(Guid? selectedBranchId)
        {
            var branches = await _lookupService.GetBranchesAsync();
            ViewBag.Branches = new SelectList(branches, "Id", "Text", selectedBranchId);
        }
    }
}