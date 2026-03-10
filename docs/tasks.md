# 🛠️ Freelance Task Board — Задачи по фазам

**Pet-проект для прокачки .NET хардскиллов**

Маркетплейс задач: заказчики публикуют задания, исполнители откликаются и берут в работу. Проект растёт вместе с обучением — каждая фаза добавляет новый слой поверх предыдущего.

## Стек

`ASP.NET Core` `PostgreSQL` `Redis` `RabbitMQ` `Docker` `GitHub Actions`
`MediatR` `EF Core` `Serilog` `OpenTelemetry` `Prometheus` `Grafana` `ELK`

---

## Структура репозитория

```
freelance-task-board/
├── src/
│   ├── TaskBoard.Domain/
│   ├── TaskBoard.Application/
│   ├── TaskBoard.Infrastructure/
│   ├── TaskBoard.Api/
│   └── TaskBoard.Notifications/
├── tests/
│   ├── TaskBoard.UnitTests/
│   └── TaskBoard.IntegrationTests/
├── docker/
├── docs/adr/
├── docker-compose.yml
├── PLAN.md
└── TASKS.md
```

---

## ⚡ Фаза 1 — Async/Await & многопоточность
`Месяц 1–2`

---

### #1 Создать solution и структуру проектов
- [x] задача выполнена

**Подзадачи:**
- [x] `dotnet new sln -n freelance-task-board`
- [x] Создать проекты: `webapi`, два `classlib`, `worker`, два `xunit`
- [x] Добавить все проекты в solution через `dotnet sln add`
- [x] Настроить project references согласно Dependency Rule (Domain ← Application ← Infrastructure ← Api)
- [x] Проверить что `dotnet build` проходит без ошибок
- [x] Добавить `.gitignore` через `dotnet new gitignore`
- [x] Первый коммит в GitHub

**Definition of Done:**
- `dotnet build` зелёный
- Solution открывается в Rider/VS без ошибок
- Репо запушено на GitHub

**На что обратить внимание:**
- Domain не должен ссылаться ни на один другой проект — это нарушение Dependency Rule
- Notifications — это `Worker Service`, не `classlib` и не `webapi`

**Ссылки:** [dotnet sln](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-sln) · [dotnet new templates](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new)

---

### #2 Минимальный API: создать задачу (in-memory)
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать модель `FreelanceTask` с полями: Id, Title, Description, Status, CreatedAt
- [ ] Реализовать `POST /tasks`, `GET /tasks`, `GET /tasks/{id}`
- [ ] Хранить в `List<FreelanceTask>` (singleton в DI) — БД пока нет
- [ ] Добавить базовую валидацию: Title не пустой

**Definition of Done:**
- Три endpoint'а отвечают корректно
- Swagger доступен на `/swagger`
- Данные живут между запросами (singleton)

**На что обратить внимание:**
- Не усложнять — никакого EF, никаких репозиториев пока. Цель — запустить скелет
- Minimal API vs Controller — выбери что ближе, Minimal API проще для старта

**Ссылки:** [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)

---

### #3 NotificationService на Channel\<T\>
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать модель `Notification` (Type, Payload, CreatedAt)
- [ ] Создать `INotificationChannel` с методами `WriteAsync` и `ReadAllAsync`
- [ ] Реализовать через `Channel<Notification>.CreateUnbounded()`
- [ ] Зарегистрировать как singleton в DI
- [ ] Написать тест: записать 3 уведомления, прочитать — получить все 3

**Definition of Done:**
- Channel работает как очередь: producer пишет, consumer читает
- Нет прямой зависимости на конкретный тип Channel — только через интерфейс

**На что обратить внимание:**
- `CreateUnbounded()` — бесконечный буфер, подходит для старта
- `WriteAsync` vs `TryWrite` — первый ждёт места, второй возвращает false

**Ссылки:** [System.Threading.Channels](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels) · [There Is No Thread](https://blog.stephencleary.com/2013/11/there-is-no-thread.html)

---

### #4 Реализовать отправку через BackgroundService
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `NotificationWorker : BackgroundService` в `TaskBoard.Notifications`
- [ ] В `ExecuteAsync` читать из Channel через `await foreach`
- [ ] Логировать каждое уведомление через `ILogger` (имитация отправки)
- [ ] Зарегистрировать через `AddHostedService<NotificationWorker>()`

**Definition of Done:**
- Воркер стартует автоматически с приложением
- Уведомление из контроллера появляется в логах воркера
- Нет `Thread.Sleep` — только `await`

**На что обратить внимание:**
- `ExecuteAsync` должен завершаться когда `stoppingToken` отменён
- `await foreach (var item in channel.Reader.ReadAllAsync(stoppingToken))` — правильный паттерн
- Не делай `while(true)` с `await Task.Delay` — антипаттерн для Channel

**Ссылки:** [BackgroundService](https://learn.microsoft.com/en-us/dotnet/core/extensions/hosted-services)

---

### #5 Добавить CancellationToken везде
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить `CancellationToken` параметром во все `async` методы
- [ ] Пробросить токен в `Channel.WriteAsync`, `ReadAllAsync`
- [ ] В контроллерах получать токен из `HttpContext.RequestAborted`
- [ ] Убедиться что при `Ctrl+C` приложение завершается без ошибок в логах

**Definition of Done:**
- Ни один async-метод не игнорирует CancellationToken
- Graceful shutdown работает корректно

**На что обратить внимание:**
- Не создавай CancellationToken вручную там где он должен приходить снаружи
- ASP.NET Core передаёт `HttpContext.RequestAborted` автоматически если параметр называется `cancellationToken`

**Ссылки:** [Cancellation in managed threads](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)

---

### #6 Намеренно сломать дедлоком — починить
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать метод, вызывающий async через `.Result` в синхронном контексте
- [ ] Воспроизвести зависание — убедиться что приложение не отвечает
- [ ] Написать комментарий в коде: почему это происходит (SynchronizationContext, захват потока)
- [ ] Починить через `await`
- [ ] Удалить сломанный код или сохранить в ветке `experiment/deadlock`

**Definition of Done:**
- Дедлок воспроизведён и задокументирован
- Починенная версия работает
- Комментарий объясняет механизм

**На что обратить внимание:**
- В ASP.NET Core SynchronizationContext отсутствует — дедлок не воспроизведётся в контроллере. Нужен xUnit тест или консольное приложение
- `ConfigureAwait(false)` в библиотечном коде предотвращает захват контекста

**Ссылки:** [Don't Block on Async Code](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html)

---

### #7 Логирование через ILogger (структурное)
- [ ] задача выполнена

**Подзадачи:**
- [ ] Убедиться что везде используется `ILogger<T>`, не `Console.WriteLine`
- [ ] Заменить string interpolation на structured logging: `_logger.LogInformation("Task {TaskId} created", task.Id)`
- [ ] Настроить `appsettings.json`: разные уровни для разных namespace
- [ ] Добавить log scope в воркере через `_logger.BeginScope`

**Definition of Done:**
- Нет ни одного `Console.WriteLine` в production-коде
- Нет `$"Task {id}"` в вызовах логгера — только named placeholders

**На что обратить внимание:**
- `{TaskId}` в фигурных скобках — это не интерполяция, это named placeholder. Structured logging позволяет фильтровать по этому полю в Kibana
- `EnableSensitiveDataLogging()` только для dev — никогда в prod

**Ссылки:** [Logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)

---

## 🐳 Фаза 2 — Тестирование + Docker
`Месяц 2–4`

---

### #8 Dockerfile для Api (multi-stage)
- [ ] задача выполнена

**Подзадачи:**
- [ ] Написать Dockerfile с двумя стадиями: `build` (sdk) и `runtime` (aspnet)
- [ ] На стадии build: `dotnet restore` → `dotnet publish`
- [ ] На стадии runtime: скопировать только артефакты из build
- [ ] Добавить `.dockerignore`: исключить `bin/`, `obj/`, `.git/`
- [ ] Собрать: `docker build -t taskboard-api .`
- [ ] Запустить: `docker run -p 8080:8080 taskboard-api`

**Definition of Done:**
- Образ собирается без ошибок, размер < 200MB
- Swagger доступен на `http://localhost:8080/swagger`

**На что обратить внимание:**
- `dotnet/sdk` весит ~700MB, `dotnet/aspnet` ~200MB — runtime на основе aspnet
- Сначала копируй только `.csproj` и делай restore — это кэширует слой с зависимостями

**Ссылки:** [Dockerize a .NET app](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container)

---

### #9 docker-compose: Api + PostgreSQL + Redis
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `docker-compose.yml` с тремя сервисами: `api`, `postgres`, `redis`
- [ ] Для postgres: задать переменные через env, добавить volume для данных
- [ ] Настроить сеть: все сервисы в одной bridge-сети
- [ ] Connection string в api — через имя сервиса (`Host=postgres`), не localhost

**Definition of Done:**
- `docker-compose up` запускает всё без ошибок
- API отвечает на `http://localhost:8080`
- Данные postgres переживают перезапуск контейнера

**На что обратить внимание:**
- Пароли не хардкодить в `docker-compose.yml` — использовать `.env` файл (добавить в `.gitignore`)

**Ссылки:** [docker-compose reference](https://docs.docker.com/compose/compose-file/)

---

### #10 Подключить EF Core, смигрировать схему
- [ ] задача выполнена

**Подзадачи:**
- [ ] Установить: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`
- [ ] Создать `AppDbContext` с `DbSet<FreelanceTask>`
- [ ] Создать первую миграцию: `dotnet ef migrations add InitialCreate`
- [ ] Применить: `dotnet ef database update`
- [ ] Настроить автоприменение через `MigrateAsync()` в `Program.cs`
- [ ] Убрать in-memory хранилище из задачи #2

**Definition of Done:**
- Таблица `FreelanceTasks` существует в PostgreSQL
- `POST /tasks` сохраняет в БД, `GET /tasks` читает из БД

**На что обратить внимание:**
- `MigrateAsync()` при старте — ок для dev, неприемлемо в prod
- Connection string только через `IConfiguration`, не хардкодить

**Ссылки:** [EF Core with PostgreSQL](https://www.npgsql.org/efcore/) · [EF Core migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

---

### #11 Health checks в docker-compose
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить `AddHealthChecks()` в `Program.cs`, endpoint `/health`
- [ ] Добавить проверку PostgreSQL: пакет `AspNetCore.HealthChecks.NpgSql`
- [ ] Добавить проверку Redis: пакет `AspNetCore.HealthChecks.Redis`
- [ ] В docker-compose: `healthcheck` для postgres и redis, `depends_on condition: service_healthy` для api

**Definition of Done:**
- `GET /health` возвращает `{"status":"Healthy"}`
- API не стартует пока postgres и redis не станут healthy

**На что обратить внимание:**
- `depends_on` без `condition` не ждёт готовности — только факта старта контейнера
- `start_period` в healthcheck — время на инициализацию перед первой проверкой

**Ссылки:** [Health checks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

---

### #12 Первые unit-тесты доменных объектов
- [ ] задача выполнена

**Подзадачи:**
- [ ] Подключить reference `TaskBoard.Domain` в `TaskBoard.UnitTests`
- [ ] Написать тесты создания `FreelanceTask`: валидные и невалидные кейсы
- [ ] Написать тесты на изменение статуса: допустимые и недопустимые переходы
- [ ] Запустить через `dotnet test`

**Definition of Done:**
- Минимум 10 unit-тестов, все зелёные
- Тесты не используют БД, HTTP — только доменные объекты

**На что обратить внимание:**
- Если для создания объекта нужны моки — логика просочилась куда не надо
- Название теста: `MethodName_StateUnderTest_ExpectedBehavior`

**Ссылки:** [Unit testing best practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

### #13 Integration-тест: POST /tasks через WebApplicationFactory
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить `Microsoft.AspNetCore.Mvc.Testing` в `TaskBoard.IntegrationTests`
- [ ] Создать `CustomWebApplicationFactory` с EF InMemory provider
- [ ] Тест: `POST /tasks` → `201 Created` с `id` в теле
- [ ] Тест: `POST /tasks` с пустым title → `400 Bad Request`

**Definition of Done:**
- Тесты запускаются без docker-compose
- HTTP статус коды проверяются явно
- Каждый тест начинает с чистой БД

**Ссылки:** [Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

---

### #14 Testcontainers: integration-тесты с реальной PostgreSQL
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить пакет `Testcontainers.PostgreSql`
- [ ] Создать `PostgreSqlFixture`: поднимает контейнер один раз на класс тестов
- [ ] Переписать `CustomWebApplicationFactory` на реальную PostgreSQL из контейнера
- [ ] Применять миграции перед тестами, использовать `Respawn` для очистки между тестами

**Definition of Done:**
- Тесты запускаются без предустановленной PostgreSQL
- Контейнер стартует перед тестами и останавливается после
- Тест проверяет реальный SQL, не InMemory

**На что обратить внимание:**
- Первый запуск медленный — Docker скачивает образ
- `IAsyncLifetime` в xUnit — для async setup/teardown

**Ссылки:** [Testcontainers for .NET](https://dotnet.testcontainers.org/) · [Respawn](https://github.com/jbogard/Respawn)

---

### #15 Добавить RabbitMQ в docker-compose, подключить MassTransit
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить `rabbitmq:management` в docker-compose
- [ ] Установить `MassTransit.RabbitMQ`
- [ ] Создать `TaskCreatedEvent` в `TaskBoard.Application`
- [ ] Publisher: публиковать событие после создания задачи
- [ ] Consumer в `TaskBoard.Notifications`: получать событие и логировать

**Definition of Done:**
- RabbitMQ Management UI доступен на `http://localhost:15672`
- Создание задачи через API → событие в логах Notifications

**На что обратить внимание:**
- MassTransit автоматически создаёт exchange и queue — не нужно настраивать вручную
- `TaskCreatedEvent` живёт в Application слое — это публичный контракт

**Ссылки:** [MassTransit getting started](https://masstransit.io/quick-starts/rabbitmq)

---

### #16 Тест: проверить публикацию события в RabbitMQ
- [ ] задача выполнена

**Подзадачи:**
- [ ] Рассмотреть два подхода: MassTransit `InMemoryTestHarness` vs `Testcontainers.RabbitMq`
- [ ] Реализовать тест: создать задачу → проверить что `TaskCreatedEvent` опубликован
- [ ] Написать тест на Consumer: событие пришло → нужный метод вызван

**Definition of Done:**
- Тест проверяет факт публикации, а не только HTTP ответ
- Тест изолирован от реального RabbitMQ

**Ссылки:** [MassTransit testing](https://masstransit.io/documentation/concepts/testing)

---

## 🚀 Фаза 3 — CI/CD & Деплой
`Месяц 4–5`

---

### #17 CI workflow: build + test
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `.github/workflows/ci.yml`
- [ ] Триггеры: push на `main`, pull_request на `main`
- [ ] Шаги: checkout → setup-dotnet → restore с кэшем NuGet → build → test
- [ ] Сохранить test results как артефакт

**Definition of Done:**
- Workflow запускается на каждый PR
- Падающий тест делает workflow красным
- NuGet кэшируется между запусками

**На что обратить внимание:**
- Testcontainers требует Docker на runner — GitHub-hosted runners имеют Docker
- Ключ кэша: `${{ hashFiles('**/*.csproj') }}` — инвалидируется при изменении зависимостей

**Ссылки:** [GitHub Actions quickstart](https://docs.github.com/en/actions/quickstart) · [actions/cache](https://github.com/actions/cache)

---

### #18 CD: сборка образа и push в GHCR
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить job `docker` после успешного `test`
- [ ] Использовать `docker/build-push-action`
- [ ] Аутентификация через встроенный `GITHUB_TOKEN`
- [ ] Тегировать: `latest` + sha коммита

**Definition of Done:**
- После merge в main образ появляется в `ghcr.io/username/taskboard-api`
- На PR — образ собирается, но не пушится

**На что обратить внимание:**
- `docker/metadata-action` удобен для автогенерации тегов
- Видимость пакета в GHCR по умолчанию `private`

**Ссылки:** [Publishing Docker images](https://docs.github.com/en/actions/publishing-packages/publishing-docker-images)

---

### #19 Поднять VPS, настроить SSH-доступ
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать VPS (Hetzner/DigitalOcean, Ubuntu 22.04, 2GB RAM)
- [ ] Добавить SSH-ключ, создать non-root пользователя с sudo
- [ ] Настроить ufw: открыть 22, 80, 443
- [ ] Установить Docker, добавить пользователя в группу `docker`

**Definition of Done:**
- Вход по SSH-ключу без пароля
- `docker run hello-world` работает без sudo

**На что обратить внимание:**
- Отключить парольную аутентификацию: `PasswordAuthentication no` в `/etc/ssh/sshd_config`

**Ссылки:** [Initial server setup Ubuntu](https://www.digitalocean.com/community/tutorials/initial-server-setup-with-ubuntu-22-04)

---

### #20 nginx + SSL через Let's Encrypt
- [ ] задача выполнена

**Подзадачи:**
- [ ] Установить nginx, настроить `proxy_pass` на `http://localhost:8080`
- [ ] Установить certbot, получить сертификат: `certbot --nginx -d yourdomain.com`
- [ ] Проверить автообновление: `certbot renew --dry-run`

**Definition of Done:**
- `https://yourdomain.com` открывает Swagger
- HTTP редиректит на HTTPS
- Сертификат валиден

**Ссылки:** [nginx reverse proxy](https://docs.nginx.com/nginx/admin-guide/web-server/reverse-proxy/) · [Certbot](https://certbot.eff.org/instructions)

---

### #21 Deploy workflow: SSH → docker pull → up
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить в GitHub Secrets: `SSH_PRIVATE_KEY`, `SSH_HOST`, `SSH_USER`
- [ ] Создать `.github/workflows/deploy.yml`, триггер: после успешного CI на main
- [ ] Использовать `appleboy/ssh-action`: `docker pull` + `docker-compose up -d`

**Definition of Done:**
- `git push` → тесты → образ → деплой на VPS за ~3 минуты
- Секреты не видны в логах workflow

**Ссылки:** [appleboy/ssh-action](https://github.com/appleboy/ssh-action)

---

### #22 Сканирование образа на уязвимости
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить `aquasecurity/trivy-action` после сборки образа
- [ ] Настроить сканирование на `HIGH` и `CRITICAL`
- [ ] Определить политику: падать или только репортировать

**Definition of Done:**
- Отчёт об уязвимостях появляется в логах CI

**Ссылки:** [trivy-action](https://github.com/aquasecurity/trivy-action)

---

### #23 Бейдж статуса pipeline в README
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `README.md` с описанием проекта и стека
- [ ] Добавить CI badge: `![CI](https://github.com/user/repo/actions/workflows/ci.yml/badge.svg)`
- [ ] Описать как запустить локально: `git clone` → `docker-compose up`

**Definition of Done:**
- README с зелёным бейджем открывается в GitHub
- Другой разработчик может запустить проект за 5 минут

---

## 🏗️ Фаза 4 — Clean Architecture, CQRS, DDD
`Месяц 5–8`

---

### #24 Перенести логику в слои Domain / Application
- [ ] задача выполнена

**Подзадачи:**
- [ ] Убрать бизнес-логику из контроллеров в Application слой
- [ ] Интерфейс `ITaskRepository` в Application, реализация в Infrastructure
- [ ] Проверить: ни один проект Domain или Application не ссылается на Infrastructure

**Definition of Done:**
- `dotnet build` зелёный
- Application.csproj не содержит ссылки на Npgsql или EntityFrameworkCore
- Контроллер только делегирует — никакой логики

**Ссылки:** [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

### #25 Выделить Aggregate: FreelanceTask
- [ ] задача выполнена

**Подзадачи:**
- [ ] Сделать сеттеры приватными: `public string Title { get; private set; }`
- [ ] Создать фабричный метод `FreelanceTask.Create(title, description)` с валидацией
- [ ] Добавить метод `Accept(executorId)` с бизнес-правилами
- [ ] Нельзя принять уже принятую задачу, нельзя создать с пустым Title

**Definition of Done:**
- Невозможно создать невалидный объект через конструктор
- Бизнес-правила живут в методах самого объекта
- EF Core работает с приватными сеттерами через Fluent API

**Ссылки:** [DDD Aggregates](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/net-core-microservice-domain-model)

---

### #26 Добавить Value Objects: Money, TaskStatus
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `Money` (Amount, Currency) с переопределёнными `Equals` и `GetHashCode`
- [ ] Добавить `Budget` типа `Money` в `FreelanceTask`
- [ ] Настроить маппинг EF Core: `OwnsOne` для Money

**Definition of Done:**
- `new Money(100, "USD") == new Money(100, "USD")` → `true`
- EF Core сохраняет Money как два столбца в той же таблице

**На что обратить внимание:**
- Value Object иммутабелен — нет сеттеров
- `record` в C# — удобный способ реализовать VO

**Ссылки:** [Value Objects](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-value-objects)

---

### #27 Реализовать CreateTaskCommand через MediatR
- [ ] задача выполнена

**Подзадачи:**
- [ ] Установить MediatR, зарегистрировать через `AddMediatR`
- [ ] Создать `CreateTaskCommand` и `CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Guid>`
- [ ] В контроллере: `await _mediator.Send(new CreateTaskCommand(...))`

**Definition of Done:**
- Контроллер не содержит логики — только `mediator.Send()`
- Handler тестируется изолированно через unit-тест с мок-репозиторием

**Ссылки:** [MediatR](https://github.com/jbogard/MediatR)

---

### #28 Реализовать GetTasksQuery с фильтрацией
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `GetTasksQuery` (Status, SearchText, PageNumber, PageSize)
- [ ] Создать `TaskDto` — плоская read-модель, не доменная сущность
- [ ] Handler: проекция через `.Select()`, вернуть `PagedResult<TaskDto>`

**Definition of Done:**
- `GET /tasks?status=Open&page=1&pageSize=20` работает
- Нет `SELECT *` — только нужные поля (проверить через EF logging)
- `AsNoTracking()` на всех read-запросах

---

### #29 Pipeline Behavior: логирование всех команд
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>`
- [ ] Логировать: название команды, время выполнения, успех/ошибка
- [ ] Зарегистрировать через `AddMediatR(cfg => cfg.AddBehavior<...>())`

**Definition of Done:**
- Каждая команда автоматически логируется без изменения handler'ов
- В логах видно название команды и время выполнения в мс

**На что обратить внимание:**
- Порядок регистрации Behaviors = порядок выполнения
- Входные параметры команды логировать осторожно — могут быть sensitive данные

---

### #30 Pipeline Behavior: валидация через FluentValidation
- [ ] задача выполнена

**Подзадачи:**
- [ ] Установить FluentValidation
- [ ] Создать `CreateTaskCommandValidator` с правилами
- [ ] Создать `ValidationBehavior`: запускает validators, при ошибке бросает `ValidationException`
- [ ] Глобальный exception handler: `ValidationException` → `400 Bad Request`

**Definition of Done:**
- `POST /tasks` с пустым Title → `400` с описанием ошибки
- Валидация в pipeline, не в handler'е

**Ссылки:** [FluentValidation](https://docs.fluentvalidation.net/en/latest/)

---

### #31 Domain Event: TaskAccepted → уведомление
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `IDomainEvent` и поднимать `TaskAcceptedEvent` в `FreelanceTask.Accept()`
- [ ] В `AppDbContext.SaveChangesAsync` — диспетчеризировать события через MediatR после сохранения
- [ ] `TaskAcceptedEventHandler` публикует integration event в RabbitMQ
- [ ] Тест: вызов `Accept()` → в MediatR улетает `TaskAcceptedEvent`

**Definition of Done:**
- Принятие задачи автоматически триггерит уведомление
- Событие диспетчеризируется после `SaveChanges` — данные уже в БД
- Domain Event не зависит от инфраструктуры

**На что обратить внимание:**
- Domain Event внутри транзакции, Integration Event — после. Это важное различие

---

### #32 Реализовать Bid (заявка) как отдельный Aggregate
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `Bid` Aggregate Root (Id, TaskId, ExecutorId, ProposedBudget, Status)
- [ ] `Bid` ссылается на `FreelanceTask` только через `TaskId` (Guid)
- [ ] Правило: нельзя подать заявку на свою задачу
- [ ] Правило: нельзя подать заявку на задачу не в статусе `Open`

**Definition of Done:**
- Два Aggregate нет прямых навигационных свойств между собой
- Тесты покрывают оба бизнес-правила

**На что обратить внимание:**
- Агрегаты общаются через ID — ключевой паттерн DDD

---

### #33 Написать ADR-001: почему Clean Architecture
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `docs/adr/001-clean-architecture.md`
- [ ] Структура: Контекст → Решение → Альтернативы → Последствия
- [ ] Честно описать известные недостатки (boilerplate, сложность для простых операций)

**Definition of Done:**
- ADR написан, закоммичен, открывается в GitHub
- Описаны trade-offs, не только плюсы

**Ссылки:** [ADR template](https://adr.github.io/madr/)

---

## 🔍 Фаза 5 — SQL & производительность
`Месяц 8–10`

---

### #34 Включить EF Core query logging, найти N+1
- [ ] задача выполнена

**Подзадачи:**
- [ ] Включить подробный EF Core logging в `appsettings.Development.json`
- [ ] Добавить тестовые данные: 10 задач, каждая с 3 заявками
- [ ] Вызвать endpoint, зафиксировать SQL в логах, убедиться что N+1 есть

**Definition of Done:**
- N+1 задокументирован: вставка SQL в комментарий к коммиту

**На что обратить внимание:**
- `EnableSensitiveDataLogging()` — показывает параметры запросов. Только для dev!

**Ссылки:** [EF Core logging](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/simple-logging)

---

### #35 Починить N+1 через Include и проекции
- [ ] задача выполнена

**Подзадачи:**
- [ ] Исправить через `.Include()` — убедиться что один запрос
- [ ] Исправить через `.Select()` проекцию — только нужные поля
- [ ] Сравнить два подхода: когда Include лучше, когда проекция

**Definition of Done:**
- Один SQL запрос вместо N+1
- Проекция без `SELECT *`

---

### #36 Добавить поиск и фильтры
- [ ] задача выполнена

**Подзадачи:**
- [ ] `SearchText` → `.Where(t => t.Title.Contains(searchText))` → проверить SQL
- [ ] Фильтры по Status, MinBudget, MaxBudget
- [ ] Null-фильтры игнорируются (нет лишних WHERE)

**Definition of Done:**
- `GET /tasks?search=design&status=Open&minBudget=100` работает корректно

---

### #37 Проанализировать EXPLAIN ANALYZE для 3 запросов
- [ ] задача выполнена

**Подзадачи:**
- [ ] Выбрать 3 запроса: список с фильтрами, поиск, заявки по задаче
- [ ] Выполнить `EXPLAIN ANALYZE` в psql или pgAdmin
- [ ] Найти `Seq Scan`, зафиксировать в `docs/sql-analysis.md`

**Definition of Done:**
- Три execution plan зафиксированы
- Определены кандидаты для индексов

**Ссылки:** [PostgreSQL EXPLAIN](https://www.postgresql.org/docs/current/sql-explain.html) · [explain.dalibo.com](https://explain.dalibo.com)

---

### #38 Добавить индексы по горячим полям
- [ ] задача выполнена

**Подзадачи:**
- [ ] Индекс по `Status` через EF миграцию: `HasIndex(t => t.Status)`
- [ ] Составной индекс по `Status + CreatedAt`
- [ ] Индекс по `Bid.TaskId`
- [ ] Повторить `EXPLAIN ANALYZE` — убедиться что `Index Scan` вместо `Seq Scan`

**Definition of Done:**
- Execution plan показывает Index Scan для отфильтрованных запросов
- Индексы добавлены через миграцию, не ручным SQL

**На что обратить внимание:**
- Индекс ускоряет чтение, замедляет запись — не добавлять на каждое поле

---

### #39 Реализовать keyset-пагинацию
- [ ] задача выполнена

**Подзадачи:**
- [ ] Реализовать offset-пагинацию через `Skip/Take`, зафиксировать SQL
- [ ] Реализовать keyset через `WHERE id > lastId ORDER BY id LIMIT n`
- [ ] Добавить `cursor` в ответ API
- [ ] Сравнить производительность на 10 000 записей через `EXPLAIN ANALYZE`

**Definition of Done:**
- API поддерживает keyset-пагинацию через `cursor` параметр
- Keyset быстрее offset на большой таблице (зафиксировать)

**На что обратить внимание:**
- Offset: `SKIP 10000` = база читает 10000 строк. Keyset читает только нужные

---

### #40 AsNoTracking в read-запросах
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить `.AsNoTracking()` во все Query handler'ы
- [ ] Убедиться что write-операции (команды) не используют AsNoTracking

**Definition of Done:**
- Ни один Query handler не использует tracked entities

---

### #41 Сложный запрос через Dapper
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить Dapper
- [ ] Реализовать `GET /stats`: количество задач по статусам, средний бюджет, топ-5 исполнителей
- [ ] Использовать `IDbConnection` (тот же connection что у EF)
- [ ] Написать integration-тест

**Definition of Done:**
- Endpoint возвращает статистику через один SQL запрос
- Dapper и EF используют одно соединение

**Ссылки:** [Dapper](https://github.com/DapperLib/Dapper)

---

### #42 Кэшировать горячие запросы в Redis
- [ ] задача выполнена

**Подзадачи:**
- [ ] `AddStackExchangeRedisCache`, кэшировать `/stats` с TTL 60 секунд
- [ ] Cache-aside: get → miss → load → set → return
- [ ] Инвалидация: при создании задачи очищать кэш статистики

**Definition of Done:**
- Второй запрос к `/stats` не обращается к БД (проверить через EF logging)
- Кэш инвалидируется после создания задачи

---

## 📊 Фаза 6 — ELK Stack + Observability
`Месяц 10–12`

---

### #43 Поднять ELK в docker-compose
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить: `elasticsearch`, `kibana`, `filebeat`
- [ ] Elasticsearch: `discovery.type=single-node`, ограничить heap 512MB
- [ ] Filebeat: сбор логов из stdout контейнеров
- [ ] Проверить: логи приложения в Kibana → Discover

**Definition of Done:**
- Kibana открывается на `http://localhost:5601`
- Логи от `taskboard-api` видны в Discover

**На что обратить внимание:**
- `vm.max_map_count=262144` — нужно установить на хосте, иначе Elasticsearch не стартует

**Ссылки:** [Run ELK on Docker](https://www.elastic.co/guide/en/elastic-stack-get-started/current/get-started-docker.html)

---

### #44 Переключить Serilog на Elasticsearch sink
- [ ] задача выполнена

**Подзадачи:**
- [ ] Установить `Serilog.AspNetCore`, `Serilog.Sinks.Elasticsearch`
- [ ] Настроить `UseSerilog()` в `Program.cs`
- [ ] Проверить что поля `@timestamp`, `level`, `message` маппятся корректно

**Definition of Done:**
- Логи в Kibana структурированные, не просто строки
- Уровень лога фильтруется в Kibana

---

### #45 Добавить enrichers: RequestId, UserId
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить `RequestId` enricher — каждый HTTP запрос получает уникальный id
- [ ] Добавить `UserId` из JWT claims
- [ ] Убедиться что все логи одного запроса содержат одинаковый RequestId

**Definition of Done:**
- В Kibana можно отфильтровать все логи запроса по `RequestId`

---

### #46 Kibana Dashboard: ошибки и время ответа
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать Index Pattern для логов приложения
- [ ] Визуализация: количество ошибок по времени
- [ ] Визуализация: топ endpoint'ов по количеству запросов
- [ ] Alert: если ошибок > 10 за минуту

**Definition of Done:**
- Dashboard обновляется в реальном времени
- Можно найти конкретную ошибку за последний час

---

### #47 Добавить Prometheus + Grafana в docker-compose
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить `prometheus` и `grafana` в docker-compose
- [ ] Установить `prometheus-net.AspNetCore`, добавить `/metrics` endpoint
- [ ] Настроить `prometheus.yml`: scrape config для api
- [ ] В Grafana: добавить Prometheus как data source

**Definition of Done:**
- `http://localhost:9090` — Prometheus с данными от api
- Grafana открывается на `http://localhost:3000`

---

### #48 Кастомные метрики
- [ ] задача выполнена

**Подзадачи:**
- [ ] Counter `tasks_created_total`
- [ ] Counter `bids_submitted_total`
- [ ] Histogram `task_processing_duration_seconds`
- [ ] Gauge `active_tasks_count`

**Definition of Done:**
- Метрики видны в Prometheus после соответствующих действий
- Названия соответствуют Prometheus naming conventions

**Ссылки:** [Prometheus metric types](https://prometheus.io/docs/concepts/metric_types/)

---

### #49 Grafana Dashboard: HTTP, БД, очередь
- [ ] задача выполнена

**Подзадачи:**
- [ ] Импортировать готовый dashboard для ASP.NET Core (ID: 10915)
- [ ] Создать панель: RPS по endpoint'ам
- [ ] Создать панель: latency percentiles (p50, p95, p99)
- [ ] Alert: если p99 latency > 1s

**Definition of Done:**
- Dashboard показывает реальную нагрузку
- Алерт срабатывает при искусственной задержке

---

### #50 Добавить OpenTelemetry трейсинг + Jaeger
- [ ] задача выполнена

**Подзадачи:**
- [ ] Добавить `jaeger` в docker-compose (`jaegertracing/all-in-one`)
- [ ] Установить OTel пакеты, настроить в `Program.cs`: ASP.NET Core + HttpClient + EF Core
- [ ] Убедиться что трейсы появляются в Jaeger UI

**Definition of Done:**
- В Jaeger виден трейс для каждого HTTP запроса со spans: HTTP → MediatR → EF Core
- Jaeger открывается на `http://localhost:16686`

**Ссылки:** [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/dotnet/getting-started/)

---

### #51 Проследить запрос: API → App → БД → очередь
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать задачу, найти трейс в Jaeger, убедиться что все spans присутствуют
- [ ] Добавить `TraceId` в Serilog LogContext
- [ ] Проверить: по TraceId из лога можно найти трейс в Jaeger

**Definition of Done:**
- Один запрос виден насквозь: лог в Kibana → трейс в Jaeger
- `TraceId` совпадает в обоих местах

---

## 🎯 Фаза 7 — System Design & Tech Leadership
`Месяц 12–14`

---

### #52 Выделить NotificationService в отдельный процесс
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать отдельный Dockerfile для `TaskBoard.Notifications`
- [ ] Добавить сервис в docker-compose
- [ ] Api и Notifications общаются только через RabbitMQ — нет прямых HTTP вызовов
- [ ] Обновить GitHub Actions: собирать и пушить два образа

**Definition of Done:**
- `docker-compose up` запускает два независимых сервиса
- Остановка Notifications не роняет Api

---

### #53 Реализовать Outbox Pattern
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать таблицу `OutboxMessages` (Id, Type, Payload, CreatedAt, ProcessedAt)
- [ ] Domain Events писать в OutboxMessages в той же транзакции что и данные
- [ ] BackgroundService: читает необработанные сообщения, публикует в RabbitMQ, помечает
- [ ] Проверить: при сбое после сохранения — сообщение доходит при следующем запуске

**Definition of Done:**
- Domain Event и Outbox запись в одной транзакции
- At-least-once delivery работает

**На что обратить внимание:**
- Outbox даёт at-least-once — Consumer должен быть идемпотентным

**Ссылки:** [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)

---

### #54 Добавить Polly: Retry + Circuit Breaker
- [ ] задача выполнена

**Подзадачи:**
- [ ] Retry policy: 3 попытки с exponential backoff
- [ ] Circuit Breaker: после 5 ошибок подряд — разомкнуть на 30 секунд
- [ ] Метрика: количество срабатываний Circuit Breaker

**Definition of Done:**
- Retry логирует попытки
- Circuit Breaker быстро возвращает ошибку вместо ожидания таймаута

**Ссылки:** [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/)

---

### #55 Написать ADR-002: почему Outbox
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `docs/adr/002-outbox-pattern.md`
- [ ] Описать проблему, решение, альтернативы (distributed transactions, saga), trade-offs

---

### #56 Написать ADR-003: монолит vs микросервисы
- [ ] задача выполнена

**Подзадачи:**
- [ ] Создать `docs/adr/003-monolith-vs-microservices.md`
- [ ] Объяснить почему старт с модульного монолита, когда декомпозиция оправдана

---

### #57 Добавить rate limiting в API
- [ ] задача выполнена

**Подзадачи:**
- [ ] Использовать встроенный Rate Limiting Middleware (.NET 7+)
- [ ] Fixed Window: 100 запросов в минуту на IP
- [ ] Отдельная политика для `POST /tasks`: 10 в минуту
- [ ] `429 Too Many Requests` с заголовком `Retry-After`

**Definition of Done:**
- При 101-м запросе — 429
- `Retry-After` присутствует в ответе

**Ссылки:** [Rate limiting middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)

---

### #58 Написать README с архитектурной схемой
- [ ] задача выполнена

**Подзадачи:**
- [ ] Описание проекта, цель, стек
- [ ] C4 Container Diagram в Mermaid
- [ ] Секция "Как запустить": `git clone` → `docker-compose up` → Swagger
- [ ] Секция "Observability": Kibana, Grafana, Jaeger — где смотреть что

**Definition of Done:**
- Другой разработчик запускает проект за 5 минут по README
- Архитектурная схема рендерится в GitHub

**Ссылки:** [Mermaid в GitHub](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams)

---

### #59 Провести self-review: что бы сделал иначе
- [ ] задача выполнена

**Подзадачи:**
- [ ] Пройти по всему коду, выписать сомнительные места
- [ ] Открыть GitHub Issues на технический долг
- [ ] Написать `docs/retrospective.md`: 3 решения которые принял бы иначе и почему
- [ ] Сформулировать следующие шаги

**Definition of Done:**
- `docs/retrospective.md` написан и закоммичен
- GitHub Issues на технический долг открыты

---

## Сводка

| Фаза | Тема | Задач | Ключевые технологии |
|---|---|---|---|
| 1 | Async/Await | 7 | Channel\<T\>, BackgroundService, CancellationToken |
| 2 | Тестирование + Docker | 9 | xUnit, Moq, Testcontainers, docker-compose, MassTransit |
| 3 | CI/CD & Деплой | 7 | GitHub Actions, GHCR, nginx, Let's Encrypt, VPS |
| 4 | Clean Architecture, CQRS, DDD | 10 | MediatR, FluentValidation, Aggregate, Value Object, Domain Events |
| 5 | SQL & производительность | 9 | EF Core, EXPLAIN ANALYZE, индексы, Dapper, Redis |
| 6 | ELK + Observability | 9 | Serilog, Elasticsearch, Kibana, Prometheus, Grafana, OpenTelemetry |
| 7 | System Design & Leadership | 8 | Outbox, Polly, rate limiting, ADR |
| **Итого** | | **59** | |

---

*Первый шаг — задача #1: создать solution и структуру проектов.*