using MessengerApp.Data;
using MessengerApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Components;

// Елемент списку чату для бічної панелі.
public class ChatListItem
{
    public int Id { get; set; }
    public string DisplayTitle { get; set; } = string.Empty;
    public ChatType Type { get; set; }
    public int MessageCount { get; set; }
    public string? ImageData { get; set; }
}

// Список чатів для постійної бічної панелі (рендериться в _Layout на всіх сторінках).
// Звичайний користувач бачить лише свої чати; адмін бачить ВСІ.
public class ChatListViewComponent : ViewComponent
{
    private readonly MessengerContext _context;
    private readonly UserManager<AppUser> _userManager;

    public ChatListViewComponent(MessengerContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var myId = _userManager.GetUserId(HttpContext.User);
        var isAdmin = HttpContext.User.IsInRole("Admin");

        var query = _context.Chats
            .Include(c => c.Messages)
            .Include(c => c.Members).ThenInclude(m => m.User)
            .AsQueryable();

        if (!isAdmin)
        {
            // Глобальний чат бачать усі; решту — лише де користувач є учасником.
            query = query.Where(c => c.Type == ChatType.Global || c.Members.Any(m => m.UserId == myId));
        }

        var chats = await query.ToListAsync();

        // Глобальний чат — завжди зверху, далі групи/приватні за заголовком.
        var items = chats
            .Select(c => new ChatListItem
            {
                Id = c.Id,
                Type = c.Type,
                MessageCount = c.Messages.Count,
                ImageData = c.ImageData,
                DisplayTitle = ChatTitleHelper.DisplayTitle(c, myId ?? string.Empty)
            })
            .OrderBy(c => c.Type == ChatType.Global ? 0 : 1)
            .ThenBy(c => c.DisplayTitle)
            .ToList();

        int.TryParse(HttpContext.Request.Query["chatId"], out var activeId);
        ViewBag.ActiveChatId = activeId;
        return View(items);
    }
}
