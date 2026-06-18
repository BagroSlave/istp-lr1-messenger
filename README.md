# ІСтаТП · Лабораторна робота 1 — Месенджер (ASP.NET Core MVC)

**Виконавець:** Костащук Ярослав Васильович, група К26 · **Викладач:** Панченко Т. В.

Веб-застосунок **«Месенджер»**: користувачі спілкуються в чатах, є приватні листування, ролі, аватари та картинки.

## 🛠 Технології
ASP.NET Core **MVC** · **EF Core** (Code-First, міграції) · **PostgreSQL** (Npgsql) · **ASP.NET Identity** · **ClosedXML** (Excel) · **Chart.js** · Bootstrap.

## ▶️ Як запустити
1. Підняти PostgreSQL у Docker:
   `docker run -d --name istp-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=messenger -p 5432:5432 postgres:16`
   (наступного разу — просто `docker start istp-postgres`).
2. `cd app && dotnet run` → відкрити надрукований URL.
3. Вхід: **admin** / `Admin123!` (адміністратор) або **user** / `User123!`. Рядок підключення — в `app/appsettings.json`.

## 🗂 Предметна область
Месенджер. Сутності: **AppUser** (користувач), **Chat** (чат), **ChatMember** (учасник чату), **Message** (повідомлення) + ролі Identity.

---

## 📋 Пояснення етапів

### Етап 1.1 — Діаграми та база даних
Обрано власний домен «Месенджер», встановлено **PostgreSQL**. Складено діаграми:

**Діаграма прецедентів (Use-case):**
![Use-case](docs/usecase_messenger.png)

**Діаграма бази даних (ER):**
![ER](docs/er_messenger.png)

> Об'єднаний PDF: [Діаграми_ЛР1.pdf](docs/Діаграми_ЛР1.pdf)

### Етап 1.2 — Проєкт і під'єднання БД
Проєкт ASP.NET Core MVC. Доступ до даних — **EF Core + Npgsql** за підходом **Code-First**: контекст `Data/MessengerContext.cs` (успадковує `IdentityDbContext`), сутності в `Models/`. Схема створюється **міграціями** (`Migrations/`), які застосовуються на старті (`Database.Migrate()` у `Program.cs`); початкові дані — `Data/DbSeeder.cs`.

### Етап 1.3 — Контролери та представлення
Логіка у `Controllers/`: `ChatsController` (CRUD чатів), `MessagesController` (надсилання/видалення повідомлень, Excel), `AccountController` (вхід/реєстрація/профіль), `AdminController` (адмін), `ChartController`, `HomeController`. Представлення — Razor-в'юхи у `Views/`.

### Етап 1.4 — Майстер-сторінка та стилізація
`Views/Shared/_Layout.cshtml` — єдиний шаблон у стилі застосунку (Telegram/Discord): постійний **лівий сайдбар** (навігація + список чатів через ViewComponent `ChatList` + панель користувача зі спливним меню), темна тема `wwwroot/css/site.css`, скрипти `wwwroot/js/site.js`.

### Етап 1.5 — Діаграма (графік)
`ChartController` + сторінка з **Chart.js** (CDN): показує кількість повідомлень за чатами; дані передаються з контролера у форматі JSON.

### Етап 1.6 — Робота з Excel (експорт + імпорт)
`MessagesController` через **ClosedXML**: `ExportExcel` (усі повідомлення → файл `.xlsx`) та `ImportExcel` (завантажений `.xlsx` → додавання повідомлень).

### Етап 1.7 — Автентифікація, авторизація, ролі
**ASP.NET Identity**, вхід за **іменем користувача** (без email — поштовий сервер не потрібен). Ролі **Admin** / **User**. Сторінки чатів під `[Authorize]`; адмінські дії — `[Authorize(Roles="Admin")]`.

---

## ✨ Додаткові можливості
- **🌐 Глобальний чат** — закріплений зверху, незнищенний, у ньому всі користувачі.
- **✉️ Приватні чати (DM)** — листування з конкретним користувачем.
- **🖼 Картинки** — у повідомленнях, **аватари** акаунтів (сторінка «Профіль») та зображення чатів.
- **🛡 Адміністратор** — бачить усі чати, може видаляти чужі повідомлення й чати.

## 🛡 Захист (ймовірні питання)
- **Code-First чи DB-first?** — Code-First: модель у коді → міграції → `Database.Migrate()` створює схему.
- **Чому вхід без email?** — Identity за `UserName`; немає поштового сервера, тож підтвердження пошти вимкнено.
- **Як працює імпорт Excel?** — `IFormFile` → ClosedXML читає рядки (`ChatId`, текст) → створює `Message`.
- **Звідки дані для графіка?** — `GroupBy` повідомлень за чатом → JSON → Chart.js.
- **Як реалізовано ролі?** — Identity-ролі Admin/User, перевірка `User.IsInRole(...)` та `[Authorize(Roles=...)]`.
