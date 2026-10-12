using CargoTrack.Business.Services.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Controllers
{
    // Neden: Müşterinin (User rolü) kendi kargo özeti. Veriyi servis, giriş yapan kullanıcıya göre süzer.
    [Authorize(Roles = "User")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _dashboardService.GetUserDashboardAsync();
            return View(model);
        }
    }
}