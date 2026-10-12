using CargoTrack.Business.Services.Cargos;
using CargoTrack.Business.Services.Lookups;
using CargoTrack.DTO.DTOs.Cargos;
using CargoTrack.DTO.DTOs.Common;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CargoTrack.WebUI.Controllers
{
    // Neden: Tek controller tüm roller için. Hangi kargoların görüneceğini servis katmanı
    // (CargoAccessService) kullanıcının rolüne göre belirliyor. Controller'da iş kuralı yok.
    [Authorize]
    public class CargoController(
        ICargoService _cargoService,
        ICargoQueryService _cargoQueryService,
        ILookupService _lookupService) : Controller
    {
        public async Task<IActionResult> Index([FromQuery] CargoFilterDto filter)
        {
            var result = await _cargoQueryService.GetPagedAsync(filter);

            // Filtre kutularının seçili kalması için
            ViewBag.Filter = filter;

            return View(result);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var cargo = await _cargoQueryService.GetDetailAsync(id);

            // Neden: Hangi form/butonların görüneceğine servis karar verir, view sadece bu bilgiye bakar.
            var actions = await _cargoService.GetAvailableActionsAsync(id);
            ViewBag.Actions = actions;

            // Neden: Merkez listesi sadece işlem yetkisi olanın formunda lazım (User'a gereksiz veri taşımayalım).
            if (actions.CanOperate)
            {
                var centers = await _lookupService.GetTransferCentersAsync();
                ViewBag.TransferCenters = new SelectList(centers, "Id", "Text");
            }

            return View(cargo);
        }

        // Neden: Kargoyu sadece Admin ve Manager oluşturabilir. User sadece takip eder (case).
        [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
        public async Task<IActionResult> Create()
        {
            var originBranches = await LoadLookupsAsync();

            // Manager'ın tek şubesi varsa otomatik seçili gelsin
            var model = new CreateCargoDto
            {
                OriginBranchId = originBranches.Count == 1 ? originBranches[0].Id : Guid.Empty
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
        public async Task<IActionResult> Create(CreateCargoDto cargoDto)
        {
            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync();
                return View(cargoDto);
            }

            await _cargoService.CreateAsync(cargoDto);

            TempData["Success"] = "Kargo oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- FAZ 8C: OPERASYON AKSİYONLARI ----------------
        // Neden: Üç aksiyon da sadece DTO'yu servise verir. Geçiş/kod/deneme kuralları serviste.
        // Yetki iki kat: burada rol, serviste kargonun şubesi.

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
        public async Task<IActionResult> ChangeStatus(ChangeCargoStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BackToDetailsWithErrors(dto.CargoId);
            }

            await _cargoService.ChangeStatusAsync(dto);

            TempData["Success"] = "Kargo durumu güncellendi.";
            return RedirectToAction(nameof(Details), new { id = dto.CargoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
        public async Task<IActionResult> VerifyDelivery(VerifyDeliveryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BackToDetailsWithErrors(dto.CargoId);
            }

            await _cargoService.VerifyDeliveryAsync(dto);

            TempData["Success"] = "Teslimat kodu doğrulandı, kargo teslim edildi.";
            return RedirectToAction(nameof(Details), new { id = dto.CargoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
        public async Task<IActionResult> ReportDeliveryFailure(ReportDeliveryFailureDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BackToDetailsWithErrors(dto.CargoId);
            }

            await _cargoService.ReportDeliveryFailureAsync(dto);

            TempData["Success"] = "Teslimat başarısızlığı kaydedildi.";
            return RedirectToAction(nameof(Details), new { id = dto.CargoId });
        }

        // Neden: Form hatalarını (boş alan gibi) alert olarak gösterip detay sayfasına döneriz.
        private IActionResult BackToDetailsWithErrors(Guid cargoId)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m));

            TempData["Error"] = string.Join(" ", errors);
            return RedirectToAction(nameof(Details), new { id = cargoId });
        }

        // Neden: Form açılırken ve hata durumunda yeniden açılırken aynı listeler lazım, tekrar yazmayalım.
        private async Task<List<LookupItemDto>> LoadLookupsAsync()
        {
            var customers = await _lookupService.GetCustomersAsync();
            var branches = await _lookupService.GetBranchesAsync();
            var originBranches = await _lookupService.GetOriginBranchesAsync();

            ViewBag.Customers = new SelectList(customers, "Id", "Text");
            ViewBag.Branches = new SelectList(branches, "Id", "Text");
            ViewBag.OriginBranches = new SelectList(originBranches, "Id", "Text");

            return originBranches;
        }
    }
}