using MessengerApp.Models;

namespace MessengerApp.Data;

// Допоміжний клас: визначає заголовок чату для відображення.
// Для приватних чатів (DM) показуємо ім'я ІНШОГО учасника, а не збережений Title.
public static class ChatTitleHelper
{
    // chat має містити завантажені Members з User (Include).
    public static string DisplayTitle(Chat chat, string currentUserId)
    {
        if (chat.Type == ChatType.Private)
        {
            var other = chat.Members.FirstOrDefault(m => m.UserId != currentUserId);
            return other?.User?.UserName ?? "Приватний чат";
        }
        return chat.Title;
    }
}
