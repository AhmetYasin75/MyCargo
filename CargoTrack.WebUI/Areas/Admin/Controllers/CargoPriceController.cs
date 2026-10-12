using CargoTrack.Business.Services.CargoPrices;
using CargoTrack.DTO.DTOs.CargoPriceDtos;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    // Neden: Fiyat sayıları Admin tarafından değiştirilir. Ekleme/silme yok (her tip+bölge için tek aktif kural gerekir).
    [Area(Area.Admin)]
    [Authorize(Roles = Roles.Admin)]
    public class CargoPriceController(ICargoPriceService _service) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var rules = await _service.GetAllAsync();
            return View(rules);
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var dto = await _service.GetByIdAsync(id);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateCargoPriceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            await _service.UpdateAsync(dto);

            TempData["Success"] = "Fiyat kuralı güncellendi. Yeni değerler sadece bundan sonra oluşturulan kargolara uygulanır.";
            return RedirectToAction(nameof(Index));
        }
    }
}