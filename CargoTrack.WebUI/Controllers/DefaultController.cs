using CargoTrack.Business.Services.Tracking;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Controllers
{
    // Neden: Giriş gerektirmeyen herkese açık takip. Veriyi kişisel bilgi içermeyen TrackingService verir.
    public class DefaultController : Controller
    {
        private readonly ITrackingService _trackingService;

        public DefaultController(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Neden: Sadece sorgulama yapıldığı için GET.
        public async Task<IActionResult> CargoDetails(string? trackCode)
        {
            var result = await _trackingService.GetByTrackCodeAsync(trackCode);

            if (result is null)
            {
                ViewBag.error = "Bu takip numarasına ait bir kargo bulunamadı.";
                return View("Index");
            }

            return View(result);
        }
    }
}