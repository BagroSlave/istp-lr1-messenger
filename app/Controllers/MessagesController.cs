using ClosedXML.Excel;
using MessengerApp.Data;
using MessengerApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Controllers;

// Контролер повідомлень: перегляд за чатом, створення, видалення, експорт/імпорт Excel.
[Authorize]
public class MessagesController : Controller
{
    private readonly MessengerContext _context;
    private readonly UserManager<AppUser> _userManager;

    public MessagesController(MessengerContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Messages?chatId=1 — список повідомлень обраного чату.
    public async Task<IActionResult> Index(int chatId)
    {
        var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == chatId);
        if (chat == null) return NotFound();

        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        // Список усіх чатів для бічної панелі (стиль Telegram/Discord).
        ViewBag.Chats = await _context.Chats
            .Include(c => c.Messages)
            .OrderBy(c => c.Title)
            .ToListAsync();
        ViewBag.Chat = chat;
        return View(messages);
    }

    // POST: Messages/Create — надсилання повідомлення в чат від поточного користувача.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int chatId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return RedirectToAction(nameof(Index), new { chatId });
        }

        var userId = _userManager.GetUserId(User)!;
        var message = new Message
        {
            ChatId = chatId,
            SenderId = userId,
            Text = text,
            SentAt = DateTime.UtcNow
        };
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { chatId });
    }

    // POST: Messages/Delete/5 — видалення повідомлення.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null) return NotFound();

        var chatId = message.ChatId;
        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { chatId });
    }

    // GET: Messages/ExportExcel — експорт усіх повідомлень у файл .xlsx (ClosedXML).
    public async Task<IActionResult> ExportExcel()
    {
        var messages = await _context.Messages
            .Include(m => m.Chat)
            .Include(m => m.Sender)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Повідомлення");

        // Заголовки таблиці.
        ws.Cell(1, 1).Value = "Id";
        ws.Cell(1, 2).Value = "ChatId";
        ws.Cell(1, 3).Value = "Чат";
        ws.Cell(1, 4).Value = "Відправник";
        ws.Cell(1, 5).Value = "Текст";
        ws.Cell(1, 6).Value = "Надіслано";
        ws.Row(1).Style.Font.Bold = true;

        // Дані.
        var row = 2;
        foreach (var m in messages)
        {
            ws.Cell(row, 1).Value = m.Id;
            ws.Cell(row, 2).Value = m.ChatId;
            ws.Cell(row, 3).Value = m.Chat?.Title ?? "";
            ws.Cell(row, 4).Value = m.Sender?.UserName ?? "";
            ws.Cell(row, 5).Value = m.Text;
            ws.Cell(row, 6).Value = m.SentAt;
            row++;
        }
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return File(content, contentType, "messages.xlsx");
    }

    // POST: Messages/ImportExcel — імпорт повідомлень із завантаженого файлу .xlsx.
    // Очікуваний формат рядків: ChatId | (Чат) | (Відправник) | Текст. Заголовок у рядку 1.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportExcel(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ImportResult"] = "Файл не вибрано.";
            return RedirectToAction(nameof(Index), new { chatId = await _context.Chats.Select(c => c.Id).FirstOrDefaultAsync() });
        }

        var currentUserId = _userManager.GetUserId(User)!;
        var imported = 0;
        int? lastChatId = null;

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheet(1);

        // Пропускаємо рядок заголовка (1), читаємо дані з рядка 2.
        foreach (var row in ws.RowsUsed().Skip(1))
        {
            // Стовпець 2 = ChatId, стовпець 5 = Текст (узгоджено з ExportExcel).
            if (!row.Cell(2).TryGetValue<int>(out var chatId))
            {
                continue;
            }
            var text = row.Cell(5).GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            // Імпортуємо лише в існуючі чати.
            if (!await _context.Chats.AnyAsync(c => c.Id == chatId))
            {
                continue;
            }

            _context.Messages.Add(new Message
            {
                ChatId = chatId,
                SenderId = currentUserId,
                Text = text,
                SentAt = DateTime.UtcNow
            });
            lastChatId = chatId;
            imported++;
        }

        await _context.SaveChangesAsync();
        TempData["ImportResult"] = $"Імпортовано повідомлень: {imported}.";

        var redirectChatId = lastChatId ?? await _context.Chats.Select(c => c.Id).FirstOrDefaultAsync();
        return RedirectToAction(nameof(Index), new { chatId = redirectChatId });
    }
}
