using Microsoft.AspNetCore.Identity;

namespace MessengerApp.Models;

// Користувач застосунку. Розширює стандартного IdentityUser (логін, email, пароль тощо).
public class AppUser : IdentityUser
{
    // Аватар користувача у вигляді data-URI (data:image/...;base64,...). Необов'язковий.
    public string? AvatarData { get; set; }

    // Навігаційні властивості: повідомлення, надіслані користувачем, та його членства в чатах.
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<ChatMember> ChatMembers { get; set; } = new List<ChatMember>();
}
