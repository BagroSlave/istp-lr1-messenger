using MessengerApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Controllers;

// Адміністративний контролер: доступний лише користувачам у ролі "Admin".
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    // GET: Admin/Users — список усіх користувачів та їхніх ролей.
    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        var model = new List<UserRow>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            model.Add(new UserRow
            {
                Email = u.Email ?? "",
                UserName = u.UserName ?? "",
                Roles = string.Join(", ", roles)
            });
        }
        return View(model);
    }
}

// Рядок таблиці користувачів для адмін-сторінки.
public class UserRow
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
}
