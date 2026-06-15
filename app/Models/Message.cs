using System.ComponentModel.DataAnnotations;

namespace MessengerApp.Models;

// Повідомлення, надіслане користувачем у конкретний чат.
public class Message
{
    public int Id { get; set; }

    [Display(Name = "Чат")]
    public int ChatId { get; set; }
    public Chat? Chat { get; set; }

    [Display(Name = "Відправник")]
    public string SenderId { get; set; } = string.Empty;
    public AppUser? Sender { get; set; }

    // Текст не обов'язковий, якщо повідомлення містить картинку (ImageData).
    [Display(Name = "Текст")]
    public string Text { get; set; } = string.Empty;

    // Зображення повідомлення у вигляді data-URI (data:image/...;base64,...). Необов'язкове.
    [Display(Name = "Зображення")]
    public string? ImageData { get; set; }

    [Display(Name = "Надіслано")]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
