using MessengerApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Реєструємо MVC (контролери + представлення).
builder.Services.AddControllersWithViews();

// Підключаємо EF Core з провайдером PostgreSQL (Npgsql).
// Рядок підключення береться з appsettings.json (ключ "DefaultConnection").
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Host=localhost;Port=5432;Database=messenger;Username=postgres;Password=postgres";
builder.Services.AddDbContext<MessengerContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Застосування міграцій та наповнення БД під час старту.
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
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
