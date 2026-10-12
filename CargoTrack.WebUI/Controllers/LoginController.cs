using CargoTrack.DTO.DTOs.UserDtos;
using CargoTrack.Entity.Entities;
using CargoTrack.WebUI.Consts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrack.WebUI.Controllers
{
    public class LoginController(SignInManager<AppUser> _signInManager,
    UserManager<AppUser> _userManager) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginUserDto loginUserDto)
        {
            var result = await _signInManager.PasswordSignInAsync(loginUserDto.UserName,
                loginUserDto.Password, false, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
                return View(loginUserDto);
            }

            var user = await _userManager.FindByNameAsync(loginUserDto.UserName);
            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Contains(Roles.Admin))
            {
                // Faz 9: Admin artık giriş sonrası kendi dashboard'una gider
                return Redirect("/Admin/Dashboard/Index");
            }

            if (userRoles.Contains(Roles.Manager))
            {
                return Redirect("/Manager/Dashboard/Index");
            }

            if (userRoles.Contains(Roles.User))
            {
                // Neden: Müşteri artık kendi özet paneline düşer.
                return Redirect("/Dashboard/Index");
            }

            return RedirectToAction("Index", "Home");
        }

        // Neden: Çıkış yapıldığında cookie silinir. POST + token ile, başka bir site linkle çıkış yaptıramasın.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }
    }
}