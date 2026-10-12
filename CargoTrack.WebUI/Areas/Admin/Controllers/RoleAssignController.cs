using CargoTrack.Business.Services.Branches;
using CargoTrack.Business.Services.Employees;
using CargoTrack.DTO.DTOs.UserDtos;
using CargoTrack.Entity.Entities;
using CargoTrack.WebUI.Consts;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CargoTrack.WebUI.Areas.Admin.Controllers
{
    [Area(Area.Admin)]
    [Authorize(Roles = Roles.Admin)]
    public class RoleAssignController(UserManager<AppUser> _userManager,
                                  RoleManager<AppRole> _roleManager,
                                  IEmployeeService _employeeService,
                                  IBranchService _branchService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var mappedUsers = users.Adapt<List<ResultUserDto>>();

            foreach (var item in mappedUsers)
            {
                var user = await _userManager.FindByIdAsync(item.Id.ToString());
                item.Roles = await _userManager.GetRolesAsync(user);
            }

            return View(mappedUsers);
        }

        [HttpGet]
        public async Task<IActionResult> AssignRole(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var roleAssignList = new List<RoleAssignDto>();

            ViewBag.fullName = string.Join(" ", user.FirstName, user.LastName);

            foreach (var role in roles)
            {
                roleAssignList.Add(new RoleAssignDto
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    RoleName = role.Name,
                    RoleExist = userRoles.Contains(role.Name)
                });
            }

            // Faz 6B: Manager için şube seçimi (kullanıcının mevcut şubesi önceden seçili gelir)
            await LoadBranchesAsync(user.Id);

            return View(roleAssignList);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(List<RoleAssignDto> model, Guid? branchId)
        {
            var userId = model.Select(x => x.UserId).FirstOrDefault();

            var user = await _userManager.FindByIdAsync(userId.ToString());

            var managerSelected = model.Any(x => x.RoleName == Roles.Manager && x.RoleExist);

            // Neden: Şubesiz Manager hiçbir kargo göremez, o yüzden Manager seçilirse şube zorunlu.
            if (managerSelected && branchId is null)
            {
                ModelState.AddModelError(string.Empty, "Manager rolü için şube seçilmelidir.");
                ViewBag.fullName = string.Join(" ", user.FirstName, user.LastName);
                await LoadBranchesAsync(userId);
                return View(model);
            }

            foreach (var assignRole in model)
            {
                if (assignRole.RoleExist)
                {
                    await _userManager.AddToRoleAsync(user, assignRole.RoleName);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, assignRole.RoleName);
                }
            }

            // Faz 6B: Manager ise şubeye bağla (Employee kaydı), değilse varsa şube bağını kaldır
            if (managerSelected)
            {
                await _employeeService.AssignToBranchAsync(userId, branchId.Value);
            }
            else
            {
                await _employeeService.RemoveFromBranchAsync(userId);
            }

            return RedirectToAction(nameof(Index));
        }

        // Neden: Şube listesini hem GET hem hata durumundaki POST'ta doldurmamız gerekiyor, tekrar yazmayalım.
        private async Task LoadBranchesAsync(Guid userId)
        {
            var branches = await _branchService.GetAllAsync();
            var currentBranchId = await _employeeService.GetBranchIdAsync(userId);
            ViewBag.Branches = new SelectList(branches, "Id", "Name", currentBranchId);
        }
    }
}