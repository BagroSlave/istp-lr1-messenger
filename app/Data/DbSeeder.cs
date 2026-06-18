using MessengerApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Data;

// Початкове наповнення бази: ролі, користувачі (за іменем, без email), демо-чат і повідомлення.
public static class DbSeeder
{
    public const string AdminUserName = "admin";
    public const string AdminPassword = "Admin123!";
    public const string UserUserName = "user";
    public const string UserPassword = "User123!";

    public const string GlobalChatTitle = "Глобальний чат";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<MessengerContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Ролі.
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2. Адміністратор.
        var admin = await userManager.FindByNameAsync(AdminUserName);
        if (admin is null)
        {
            admin = new AppUser { UserName = AdminUserName };
            await userManager.CreateAsync(admin, AdminPassword);
            await userManager.AddToRoleAsync(admin, "Admin");
            await userManager.AddToRoleAsync(admin, "User");
        }

        // 3. Звичайний користувач.
        var user = await userManager.FindByNameAsync(UserUserName);
        if (user is null)
        {
            user = new AppUser { UserName = UserUserName };
            await userManager.CreateAsync(user, UserPassword);
            await userManager.AddToRoleAsync(user, "User");
        }

        // 4. Глобальний чат: рівно ОДИН, типу Global. Кожен користувач має бути його учасником.
        var globalChat = await context.Chats.FirstOrDefaultAsync(c => c.Type == ChatType.Global);
        if (globalChat is null)
        {
            globalChat = new Chat
            {
                Title = GlobalChatTitle,
                Type = ChatType.Global,
                CreatedAt = DateTime.UtcNow
            };
            context.Chats.Add(globalChat);
            await context.SaveChangesAsync();

            // Демонстраційні повідомлення (лише при першому створенні).
            context.Messages.AddRange(
                new Message { ChatId = globalChat.Id, SenderId = admin!.Id, Text = "Привіт! Це глобальний чат.", SentAt = DateTime.UtcNow.AddMinutes(-10) },
                new Message { ChatId = globalChat.Id, SenderId = user!.Id, Text = "Вітаю! Радий приєднатися.", SentAt = DateTime.UtcNow.AddMinutes(-8) },
                new Message { ChatId = globalChat.Id, SenderId = admin.Id, Text = "Тут можуть спілкуватися всі користувачі.", SentAt = DateTime.UtcNow.AddMinutes(-5) }
            );
            await context.SaveChangesAsync();
        }

        // Додаємо до глобального чату ВСІХ користувачів, кого там ще немає.
        var allUserIds = await context.Users.Select(u => u.Id).ToListAsync();
        var existingMemberIds = await context.ChatMembers
            .Where(cm => cm.ChatId == globalChat.Id)
            .Select(cm => cm.UserId)
            .ToListAsync();

        var missing = allUserIds.Except(existingMemberIds).ToList();
        if (missing.Count > 0)
        {
            context.ChatMembers.AddRange(
                missing.Select(uid => new ChatMember { ChatId = globalChat.Id, UserId = uid }));
            await context.SaveChangesAsync();
        }
    }
}
