using MessengerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Data;

// Початкове наповнення бази демонстраційними даними.
// На цьому етапі (до автентифікації) створюємо лише глобальний чат.
public static class DbSeeder
{
    public const string GlobalChatTitle = "Глобальний чат";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<MessengerContext>();

        if (!await context.Chats.AnyAsync(c => c.Type == ChatType.Global))
        {
            context.Chats.Add(new Chat
            {
                Title = GlobalChatTitle,
                Type = ChatType.Global,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
    }
}
