using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.ComponentModel.DataAnnotations;

namespace CargoTrack.WebUI.Filters
{
    // Neden: Servislerde fırlattığımız hataları her Controller'da try/catch ile yakalamak yerine
    // tek yerde yakalıyoruz (Global Exception Handling). Bilmediğimiz hataları bilerek yakalamıyoruz,
    // onları ASP.NET kendi hata sayfasıyla işler (geliştirirken detaylı hata görürsün).
    public class GlobalExceptionFilter(ITempDataDictionaryFactory _tempDataFactory) : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                // İş kuralı hatası: kullanıcıya mesaj göster, geldiği sayfaya geri dön
                case ValidationException validationException:
                    {
                        var tempData = _tempDataFactory.GetTempData(context.HttpContext);
                        tempData["Error"] = validationException.Message;

                        context.Result = RedirectBack(context.HttpContext);
                        context.ExceptionHandled = true;
                        break;
                    }

                // Yetki hatası: girişliyse "Yetkisiz Erişim", değilse giriş sayfası
                case UnauthorizedAccessException:
                    {
                        var isLoggedIn = context.HttpContext.User.Identity?.IsAuthenticated == true;

                        context.Result = isLoggedIn
                            ? new RedirectToActionResult("AccessDenied", "ErrorPages", new { area = "" })
                            : new RedirectToActionResult("Index", "Login", new { area = "" });

                        context.ExceptionHandled = true;
                        break;
                    }
            }
        }

        // Neden: Kullanıcıyı hatanın geldiği sayfaya geri gönderiyoruz. Güvenlik için sadece aynı siteden
        // gelen adreslere yönlendiriyoruz (başka siteye yönlendirme saldırısını önlemek için).
        private static IActionResult RedirectBack(HttpContext httpContext)
        {
            var referer = httpContext.Request.Headers.Referer.ToString();

            if (Uri.TryCreate(referer, UriKind.Absolute, out var uri) &&
                uri.Host == httpContext.Request.Host.Host)
            {
                return new LocalRedirectResult(uri.PathAndQuery);
            }

            return new RedirectToActionResult("Index", "Home", new { area = "" });
        }
    }
}