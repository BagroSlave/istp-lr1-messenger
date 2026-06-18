using MessengerApp.Data;
using MessengerApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Реєструємо MVC (контролери + представлення) та Razor Pages (потрібні для Identity UI).
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Підключаємо EF Core з провайдером PostgreSQL (Npgsql).
// Рядок підключення береться з appsettings.json (ключ "DefaultConnection").
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Host=localhost;Port=5432;Database=messenger;Username=postgres;Password=postgres";
builder.Services.AddDbContext<MessengerContext>(options =>
    options.UseNpgsql(connectionString));

// ASP.NET Identity з ролями та сховищем у нашому контексті EF Core.
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        // Послаблені вимоги до пароля для навчального проєкту.
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MessengerContext>()
    .AddDefaultTokenProviders();

// Власні сторінки входу/виходу (контролер Account), без стандартного UI та email.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

var app = builder.Build();

// Застосування міграцій та наповнення БД під час старту.
// Загорнуто в try/catch, щоб відсутність живої БД (Docker вимкнено)
// не зламала запуск/каркас застосунку.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<MessengerContext>();
        db.Database.Migrate();
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Помилка під час застосування міграцій / наповнення БД. " +
                            "Переконайтеся, що PostgreSQL запущено (docker start istp-postgres).");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
