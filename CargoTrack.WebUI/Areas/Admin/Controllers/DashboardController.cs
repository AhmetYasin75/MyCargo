using CargoTrack.Business.Services.Dashboard;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    // Neden: Controller sadece servisten gelen hazır rakamları view'a verir, hesap yapmaz.
    [Area(Area.Admin)]
    [Authorize(Roles = Roles.Admin)]
    public class DashboardController(IDashboardService _service) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var dashboard = await _service.GetAdminDashboardAsync();
            return View(dashboard);
        }
    }
}