using System.ComponentModel.DataAnnotations;

namespace MessengerApp.Models;

// Членство користувача в чаті (зв'язок «багато-до-багатьох» між чатами та користувачами).
public class ChatMember
{
    public int Id { get; set; }

    [Display(Name = "Чат")]
    public int ChatId { get; set; }
    public Chat? Chat { get; set; }

    [Display(Name = "Користувач")]
    public string UserId { get; set; } = string.Empty;
    public AppUser? User { get; set; }
}
