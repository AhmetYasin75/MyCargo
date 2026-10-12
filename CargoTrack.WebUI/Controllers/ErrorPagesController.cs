using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Controllers
{
    // Neden: Program.cs'te AccessDeniedPath = "/ErrorPages/AccessDenied" olarak ayarlı.
    // Yetkisi olmayan kullanıcı bu sayfaya yönlendirilir (case'in "yetkisiz erişim" ekranı).
    public class ErrorPagesController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}