using System.ComponentModel.DataAnnotations;

namespace MessengerApp.Models;

// Тип чату: глобальний (один на весь застосунок), груповий або приватний (DM на двох).
public enum ChatType
{
    Global,
    Group,
    Private
}

// Чат (бесіда), у якому користувачі обмінюються повідомленнями.
public class Chat
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Вкажіть назву чату")]
    [Display(Name = "Назва")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Тип")]
    public ChatType Type { get; set; } = ChatType.Group;

    [Display(Name = "Створено")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Зображення (аватар) чату у вигляді data-URI (data:image/...;base64,...). Необов'язкове.
    [Display(Name = "Зображення")]
    public string? ImageData { get; set; }

    // Навігаційні властивості: повідомлення чату та його учасники.
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
}
