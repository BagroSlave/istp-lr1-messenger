using MessengerApp.Data;
using MessengerApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Controllers;

// Контролер чатів: редірект на глобальний чат, CRUD для групових чатів, створення приватних (DM).
[Authorize]
public class ChatsController : Controller
{
    private readonly MessengerContext _context;
    private readonly UserManager<AppUser> _userManager;

    public ChatsController(MessengerContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Chats — просто перенаправляємо на глобальний чат.
    public async Task<IActionResult> Index()
    {
        var globalChat = await _context.Chats.FirstOrDefaultAsync(c => c.Type == ChatType.Global);
        if (globalChat == null) return NotFound();
        return RedirectToAction("Index", "Messages", new { chatId = globalChat.Id });
    }

    // GET: Chats/Details/5 — деталі чату разом із повідомленнями.
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var chat = await _context.Chats
            .Include(c => c.Messages).ThenInclude(m => m.Sender)
            .Include(c => c.Members).ThenInclude(cm => cm.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (chat == null) return NotFound();
        return View(chat);
    }

    // GET: Chats/Create
    public IActionResult Create() => View();

    // POST: Chats/Create — створює груповий чат, де творець одразу стає учасником.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title")] Chat chat, IFormFile? image)
    {
        if (ModelState.IsValid)
        {
            chat.CreatedAt = DateTime.UtcNow;
            chat.Type = ChatType.Group;
            chat.ImageData = await ImageHelper.ToDataUriAsync(image);
            _context.Add(chat);
            await _context.SaveChangesAsync();

            // Творець стає учасником нового групового чату.
            var userId = _userManager.GetUserId(User)!;
            _context.ChatMembers.Add(new ChatMember { ChatId = chat.Id, UserId = userId });
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Messages", new { chatId = chat.Id });
        }
        return View(chat);
    }

    // GET: Chats/Edit/5 — перейменування. Глобальний чат не редагується.
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var chat = await _context.Chats.FindAsync(id);
        if (chat == null) return NotFound();
        if (chat.Type == ChatType.Global) return Forbid();
        return View(chat);
    }

    // POST: Chats/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title")] Chat input, IFormFile? image)
    {
        if (id != input.Id) return NotFound();

        var chat = await _context.Chats.FindAsync(id);
        if (chat == null) return NotFound();
        if (chat.Type == ChatType.Global) return Forbid();

        if (ModelState.IsValid)
        {
            chat.Title = input.Title;

            // Оновлюємо зображення лише якщо завантажено новий файл (інакше зберігаємо поточне).
            var dataUri = await ImageHelper.ToDataUriAsync(image);
            if (dataUri != null) chat.ImageData = dataUri;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Chats.Any(c => c.Id == chat.Id)) return NotFound();
                throw;
            }
            return RedirectToAction("Index", "Messages", new { chatId = chat.Id });
        }
        return View(chat);
    }

    // GET: Chats/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var chat = await _context.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (chat == null) return NotFound();
        // Глобальний чат видалити не можна.
        if (chat.Type == ChatType.Global) return Forbid();
        if (!await CanManageAsync(chat)) return Forbid();
        return View(chat);
    }

    // POST: Chats/Delete/5 — видалення. Глобальний чат заблоковано; решта — лише адмін або учасник.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var chat = await _context.Chats.FindAsync(id);
        if (chat == null) return RedirectToAction(nameof(Index));

        // Глобальний чат видалити не можна за жодних умов.
        if (chat.Type == ChatType.Global) return Forbid();
        if (!await CanManageAsync(chat)) return Forbid();

        // Прибираємо членства та повідомлення вручну (FK на Sender — Restrict).
        var members = _context.ChatMembers.Where(cm => cm.ChatId == id);
        var messages = _context.Messages.Where(m => m.ChatId == id);
        _context.Messages.RemoveRange(messages);
        _context.ChatMembers.RemoveRange(members);
        _context.Chats.Remove(chat);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Chats/StartDm — список користувачів для початку приватного чату.
    [HttpGet]
    public async Task<IActionResult> StartDm()
    {
        var myId = _userManager.GetUserId(User)!;
        var users = await _userManager.Users
            .Where(u => u.Id != myId)
            .OrderBy(u => u.UserName)
            .ToListAsync();
        return View(users);
    }

    // POST: Chats/StartDm — знаходить існуючий приватний чат із користувачем або створює новий.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartDm(string userId)
    {
        var myId = _userManager.GetUserId(User)!;
        if (string.IsNullOrEmpty(userId) || userId == myId)
            return RedirectToAction(nameof(StartDm));

        var other = await _userManager.FindByIdAsync(userId);
        if (other == null) return RedirectToAction(nameof(StartDm));

        // Шукаємо приватний чат, у якому учасники — рівно ці двоє.
        var existing = await _context.Chats
            .Where(c => c.Type == ChatType.Private)
            .Where(c => c.Members.Any(m => m.UserId == myId) && c.Members.Any(m => m.UserId == userId))
            .Where(c => c.Members.Count == 2)
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        if (existing != 0)
            return RedirectToAction("Index", "Messages", new { chatId = existing });

        // Створюємо новий приватний чат. Title зберігаємо як підказку, але в UI він обчислюється.
        var chat = new Chat
        {
            Title = "DM",
            Type = ChatType.Private,
            CreatedAt = DateTime.UtcNow
        };
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        _context.ChatMembers.AddRange(
            new ChatMember { ChatId = chat.Id, UserId = myId },
            new ChatMember { ChatId = chat.Id, UserId = userId }
        );
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Messages", new { chatId = chat.Id });
    }

    // Чи може поточний користувач керувати чатом (видаляти): адмін або учасник.
    private async Task<bool> CanManageAsync(Chat chat)
    {
        if (User.IsInRole("Admin")) return true;
        var myId = _userManager.GetUserId(User)!;
        return await _context.ChatMembers.AnyAsync(cm => cm.ChatId == chat.Id && cm.UserId == myId);
    }
}
