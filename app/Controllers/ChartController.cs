using MessengerApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Controllers;

// Контролер діаграми: кількість повідомлень у кожному чаті (Chart.js).
[Authorize]
public class ChartController : Controller
{
    private readonly MessengerContext _context;

    public ChartController(MessengerContext context)
    {
        _context = context;
    }

    // GET: Chart — сторінка зі стовпчиковою діаграмою «повідомлень на чат».
    public async Task<IActionResult> Index()
    {
        var data = await _context.Chats
            .Select(c => new ChartItem { Chat = c.Title, Count = c.Messages.Count })
            .ToListAsync();

        return View(data);
    }
}

// Допоміжна модель для передачі даних діаграми у представлення.
public class ChartItem
{
    public string Chat { get; set; } = string.Empty;
    public int Count { get; set; }
}
