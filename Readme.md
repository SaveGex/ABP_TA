# Conference Room Booking API

REST API для управління конференц-залами, їх бронюванням та розрахунком вартості оренди з урахуванням часових тарифів.

## Бізнес-задача

Компанія надає конференц-зали в оренду для бізнесу. API дозволяє:

- реєструвати, редагувати та видаляти зали з переліком доступних послуг (такі як: проєктор, Wi-Fi, звук і т.п.);
- шукати доступні зали за датою, часом і місткістю;
- бронювати зал з автоматичним розрахунком вартості оренди;
- отримувати аналітичні звіти по виручці та завантаженості залів.

### Ціноутворення (найважливіше бізнес-правило)

Вартість оренди залежить не від фіксованої ставки, а від того, на які тарифні періоди потрапляє бронювання:

| Період      | Коефіцієнт      |
|-------------|-----------------|
| 06:00–09:00 | -10% (знижка)   |
| 09:00–18:00 | базова ставка   |
| 12:00–14:00 | +15% (націнка, пікові години) |
| 18:00–23:00 | -20% (знижка)   |

Якщо бронювання перетинає кілька періодів (наприклад, 11:00–15:00 зачіпає і стандартні, і пікові години), вартість рахується посегментно - кожна година (чи її частка) множиться на свій коефіцієнт, після чого результати підсумовуються. Правила задані в конфігурації (`appsettings.json`, секція `Pricing`), а не захардкоджені - це дозволяє змінювати тарифну сітку без перекомпіляції.

## Технічні рішення

- **Clean Architecture** - чотири шари (`Domain` <- `Application` <- `Infrastructure` <- `MainWeb`), залежності спрямовані тільки досередини. `Domain` не залежить ні від чого зовнішнього.
- **DDD** - `Room`, `Booking` - агрегати з фабричними методами (`Room.Create`, `Booking.Create`), що інкапсулюють інваріанти. `Money` і `TimeSlot` - value objects. `Booking` навмисно окремий агрегат від `Room` (посилається на `RoomId`, а не тримає `Room` всередині) - щоб уникнути одного розрослого агрегату і зайвих блокувань при паралельних бронюваннях.
- **CQRS + MediatR** - кожна операція (`CreateRoom`, `SearchAvailableRooms`, `CreateBooking` тощо) - окремі Command/Query з власним handler і FluentValidation-валідатором, підключеним через `ValidationBehavior` у MediatR pipeline.
- **Розрахунок вартості** винесений в окремий domain service (`IRentalPricingService`, реалізація в `Infrastructure`), а не розмазаний по хендлеру чи ентіті - це дозволяє тестувати логіку ціноутворення ізольовано (`Infrastructure.UnitTests/Services/RentalPricingServiceTests.cs`).
- **Захист від подвійного бронювання** - при створенні бронювання перевіряється перетин часових слотів для того самого залу (`Slot.Start < request.slot.End && Slot.End > request.slot.Start`), конфлікт повертає `409 Conflict`.
- **EF Core + SQL Server** - `BookingDbContext` абстрагований через `IBookingDbContext` в Application-шарі, що дозволяє мокати БД у юніт-тестах без реального EF/SQL.
- **Глобальна обробка помилок** через `IExceptionHandler` (`GlobalExceptionHandler`) - уніфіковані `ProblemDetails`-відповіді замість "сирих" стектрейсів.
- **Swagger/OpenAPI** - автогенерована документація з XML-коментарями з Application та MainWeb.

## Запуск проєкту

### Варіант 1 - Docker Compose (рекомендовано, одна команда)

```bash
docker compose up --build
```

Піднімає SQL Server (з healthcheck) і API. Після старту БД автоматично створюється і засівається початковими даними (зали А/Б/В, послуги - див. `DbSpecificationInitializer`).

- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger

За потреби пароль SA можна змінити через змінну середовища перед запуском:

```bash
export DB_SA_PASSWORD="YourStrongPassword123!"
docker compose up --build
```

### Варіант 2 - локально через `dotnet run`

Потрібен розгорнутий SQL Server (локально або в контейнері) і рядок підключення в `src/MainWeb/appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=booking_db;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
}
```

Запуск:

```bash
dotnet run --project src/MainWeb
```

### Тести

```bash
dotnet test
```

## Структура проєкту

```
src/
  Domain/          - сутності, value objects, доменні сервіси (без зовнішніх залежностей)
  Application/      - CQRS команди/запити, DTO, валідатори, інтерфейси
  Infrastructure/   - EF Core, реалізації інтерфейсів, RentalPricingService, seed-дані
  MainWeb/          - контролери, middleware, DIC, Swagger
tests/
  Application.UnitTests/     - тести хендлерів commands/queries
  Infrastructure.UnitTests/  - тести RentalPricingService (сегментація тарифів)
```

## Аналітичні звіти

Реалізовано два звіти для бізнесу (`GET /api/Reports/...`):

- **RevenueReport** - виручка за період з розбивкою по залах (`RoomRevenueBreakdownDTO`): скільки заробив кожен зал окремо і сумарно.
- **UtilizationReport** - завантаженість залів за період (`RoomUtilizationDetailsDTO`): скільки годин зал був заброньований і відсоток використання від доступного часу.

## API Endpoints

| Метод | Маршрут | Опис |
|-------|---------|------|
| POST  | `/api/Rooms` | Створити зал |
| PUT   | `/api/Rooms/{id}` | Оновити зал |
| DELETE| `/api/Rooms/{id}` | Видалити зал |
| POST  | `/api/Rooms/{id}/services` | Додати послугу залу |
| GET   | `/api/Rooms/search` | Пошук доступних залів за датою/часом/місткістю |
| POST  | `/api/Bookings` | Забронювати зал (з розрахунком вартості) |
| GET   | `/api/Bookings/{id}` | Отримати бронювання за ID |
| GET   | `/api/Bookings` | Список бронювань з фільтрами |
| GET   | `/api/Reports/revenue` | Звіт по виручці |
| GET   | `/api/Reports/utilization` | Звіт по завантаженості залів |

Повна специфікація - у Swagger UI (`/swagger`) після запуску.
