using MessengerApp.Data;
using MessengerApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Controllers;

// Вхід / реєстрація / вихід за іменем користувача (без email — поштовий сервер не потрібен).
public class AccountController : Controller
{
    private readonly UserManager<AppUser> _users;
    private readonly SignInManager<AppUser> _signIn;
    private readonly MessengerContext _context;

    public AccountController(UserManager<AppUser> users, SignInManager<AppUser> signIn, MessengerContext context)
    {
        _users = users;
        _signIn = signIn;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string userName, string password, bool rememberMe = false, string? returnUrl = null)
    {
        var result = await _signIn.PasswordSignInAsync(userName, password, rememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
            return RedirectToLocal(returnUrl);

        ModelState.AddModelError(string.Empty, "Невірне ім'я користувача або пароль.");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string userName, string password, string? returnUrl = null)
    {
        var user = new AppUser { UserName = userName };
        var result = await _users.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _users.AddToRoleAsync(user, "User");

            // Кожен новий користувач автоматично стає учасником глобального чату.
            var globalChat = await _context.Chats.FirstOrDefaultAsync(c => c.Type == ChatType.Global);
            if (globalChat != null &&
                !await _context.ChatMembers.AnyAsync(cm => cm.ChatId == globalChat.Id && cm.UserId == user.Id))
            {
                _context.ChatMembers.Add(new ChatMember { ChatId = globalChat.Id, UserId = user.Id });
                await _context.SaveChangesAsync();
            }

            await _signIn.SignInAsync(user, isPersistent: false);
            return RedirectToLocal(returnUrl);
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // GET: Account/Profile — сторінка профілю з поточним аватаром та формою завантаження.
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));
        return View(user);
    }

    // POST: Account/Profile — завантаження аватара: конвертуємо у data-URI (base64) та зберігаємо.
    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(IFormFile? avatar)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        if (avatar == null || avatar.Length == 0)
        {
            TempData["ProfileResult"] = "Файл не вибрано.";
            return View(user);
        }

        var dataUri = await ImageHelper.ToDataUriAsync(avatar);
        if (dataUri == null)
        {
            TempData["ProfileResult"] = "Невірний файл. Потрібне зображення розміром до 3 МБ.";
            return View(user);
        }

        user.AvatarData = dataUri;
        await _users.UpdateAsync(user);
        TempData["ProfileResult"] = "Аватар оновлено.";
        return RedirectToAction(nameof(Profile));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
        => Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Home");
}
