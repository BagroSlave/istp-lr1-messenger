# ІСтаТП · ЛР1 «Месенджер» (ASP.NET Core MVC) — гілка етапу 1.5

**Виконавець:** Костащук Ярослав Васильович, група К26 · **Викладач:** Панченко Т. В.

Ця гілка показує стан проєкту на **етапі 1.5**. Нижче — що додано або змінено на кожному етапі (до цього включно).

## Прогрес по етапах
- **Етап 1.1** — обрано предметну область «Месенджер»; складено діаграми use-case та ER (тека `docs/`).
- **Етап 1.2** — створено проєкт ASP.NET Core MVC; підключено EF Core + PostgreSQL (Code-First): моделі `AppUser`/`Chat`/`ChatMember`/`Message`, контекст `MessengerContext`, перша міграція.
- **Етап 1.3** — додано контролери та представлення: CRUD чатів (`ChatsController`) і повідомлень (`MessagesController`) з Razor-вюхами.
- **Етап 1.4** — додано майстер-сторінку `_Layout.cshtml` і стилізацію (`wwwroot/css/site.css`).
- **Етап 1.5** — додано графік кількості повідомлень за чатами (Chart.js, `ChartController`).  ◀ **цей етап (гілка)**

## Предметна область
Месенджер. Сутності: **AppUser**, **Chat**, **ChatMember**, **Message**.

## Технології
ASP.NET Core MVC · EF Core (Code-First) · PostgreSQL (Npgsql) · ASP.NET Identity.

## Діаграми
### Use-case
![Use-case](docs/usecase_messenger.png)
### ER
![ER](docs/er_messenger.png)
