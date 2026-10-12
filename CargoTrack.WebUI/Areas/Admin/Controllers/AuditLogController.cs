using CargoTrack.Business.Services.AuditLogs;
using CargoTrack.DTO.DTOs.AuditLogDtos;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    // Neden: Audit kayıtları sadece okunur, bu controller'da ekleme/düzenleme/silme yok.
    [Area(Area.Admin)]
    [Authorize(Roles = Roles.Admin)]
    public class AuditLogController(IAuditLogService _service) : Controller
    {
        public async Task<IActionResult> Index([FromQuery] AuditLogFilterDto filter)
        {
            var result = await _service.GetPagedAsync(filter);

            // Filtre kutularının dolu kalması ve "Varlık" listesi için
            ViewBag.Filter = filter;
            ViewBag.EntityNames = await _service.GetEntityNamesAsync();

            return View(result);
        }
    }
}