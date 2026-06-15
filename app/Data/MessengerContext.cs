using MessengerApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp.Data;

// Контекст бази даних месенджера (Code-First, провайдер PostgreSQL/Npgsql).
// Успадковує IdentityDbContext — додає таблиці користувачів, ролей тощо.
public class MessengerContext : IdentityDbContext<AppUser>
{
    public MessengerContext(DbContextOptions<MessengerContext> options) : base(options)
    {
    }

    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatMember> ChatMembers => Set<ChatMember>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Обов'язково викликаємо базовий метод для налаштування таблиць Identity.
        base.OnModelCreating(builder);

        // Зв'язок: чат -> повідомлення (один чат має багато повідомлень).
        // При видаленні чату каскадно видаляємо його повідомлення.
        builder.Entity<Message>()
            .HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // Зв'язок: користувач -> повідомлення (відправник).
        // Restrict, щоб уникнути множинних каскадних шляхів видалення.
        builder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Зв'язок: чат -> учасники (членства).
        builder.Entity<ChatMember>()
            .HasOne(cm => cm.Chat)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // Зв'язок: користувач -> членства.
        builder.Entity<ChatMember>()
            .HasOne(cm => cm.User)
            .WithMany(u => u.ChatMembers)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Унікальність: користувач не може двічі бути в одному чаті.
        builder.Entity<ChatMember>()
            .HasIndex(cm => new { cm.ChatId, cm.UserId })
            .IsUnique();
    }
}
