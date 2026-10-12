using CargoTrack.Business.Services.Cities;
using CargoTrack.Business.Services.TransferCenters;
using CargoTrack.DTO.DTOs.TransferCenterDtos;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    // Neden: Controller sadece DTO'yu servise verir. Kurallar (aynı isim, kullanımdaki merkez silinemez) serviste.
    [Area(Area.Admin)]
    [Authorize(Roles = Roles.Admin)]
    public class TransferCenterController(
        ITransferCenterService _service,
        ICityService _cityService) : Controller
    {
        public async Task<IActionResult> Index([FromQuery] TransferCenterFilterDto filter)
        {
            var result = await _service.GetPagedAsync(filter);

            // Arama kutusunun dolu kalması için
            ViewBag.Filter = filter;

            return View(result);
        }

        public async Task<IActionResult> Create()
        {
            await LoadCitiesAsync();
            return View(new CreateTransferCenterDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTransferCenterDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadCitiesAsync();
                return View(dto);
            }

            await _service.CreateAsync(dto);

            TempData["Success"] = "Transfer merkezi eklendi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var dto = await _service.GetByIdAsync(id);
            await LoadCitiesAsync();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateTransferCenterDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadCitiesAsync();
                return View(dto);
            }

            await _service.UpdateAsync(dto);

            TempData["Success"] = "Transfer merkezi güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Neden: Silme veriyi değiştirir, bu yüzden GET değil POST (link tıklamasıyla yanlışlıkla silinmesin).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);

            TempData["Success"] = "Transfer merkezi silindi.";
            return RedirectToAction(nameof(Index));
        }

        // Neden: Create ve Update formlarında aynı şehir listesi lazım, tekrar yazmayalım.
        private async Task LoadCitiesAsync()
        {
            var cities = await _cityService.GetAllAsync();
            ViewBag.Cities = new SelectList(cities.OrderBy(c => c.Name), "Id", "Name");
        }
    }
}