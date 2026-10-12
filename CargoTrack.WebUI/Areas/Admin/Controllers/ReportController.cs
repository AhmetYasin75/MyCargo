using CargoTrack.Business.Services.Reports;
using CargoTrack.DTO.DTOs.ReportDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    // Neden: Rapor ekranı sadece Admin'e açık. Controller hesap yapmaz, hazır raporu view'a verir.
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index(ReportFilterDto filter)
        {
            var model = await _reportService.GetReportAsync(filter);
            return View(model);
        }
    }
}