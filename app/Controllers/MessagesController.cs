using MessengerApp.Data;
using MessengerApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Controllers;

// Контролер повідомлень: перегляд за чатом, створення, видалення.
public class MessagesController : Controller
{
    private readonly MessengerContext _context;

    public MessagesController(MessengerContext context)
    {
        _context = context;
    }

    // GET: Messages?chatId=1 — список повідомлень обраного чату.
    public async Task<IActionResult> Index(int chatId)
    {
        var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == chatId);
        if (chat == null) return NotFound();

        var messages = await _context.Messages
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        ViewBag.Chat = chat;
        return View(messages);
    }

    // POST: Messages/Create — надсилання повідомлення в чат.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int chatId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return RedirectToAction(nameof(Index), new { chatId });

        var senderId = await _context.Users.Select(u => u.Id).FirstOrDefaultAsync() ?? string.Empty;
        _context.Messages.Add(new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            Text = text,
            SentAt = DateTime.UtcNow
        });
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
}
