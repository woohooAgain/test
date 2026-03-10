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
│   ├── TaskBoard.Domain/          # Сущности, Value Objects, Domain Events, интерфейсы
│   ├── TaskBoard.Application/     # Use cases, команды/запросы (CQRS), интерфейсы сервисов
│   ├── TaskBoard.Infrastructure/  # EF Core, репозитории, Redis, RabbitMQ, email
│   ├── TaskBoard.Api/             # Контроллеры, middleware, DI-конфигурация
│   └── TaskBoard.Notifications/   # Отдельный сервис уведомлений (Consumer)
├── tests/
│   ├── TaskBoard.UnitTests/       # Unit-тесты доменной логики и Application
│   └── TaskBoard.IntegrationTests/# Тесты API через WebApplicationFactory + Testcontainers
├── docker/                        # Dockerfile для каждого сервиса
├── docs/
│   └── adr/                       # Architecture Decision Records
├── docker-compose.yml             # Вся инфраструктура: БД, Redis, RabbitMQ, ELK, Grafana
├── docker-compose.override.yml    # Локальные переопределения для разработки
├── PLAN.md                        # План прокачки
└── TASKS.md                       # Этот файл
```

---

## ⚡ Фаза 1 — Async/Await & многопоточность
`Месяц 1–2 · Стартовая точка проекта`

Создаётся скелет проекта и сервис уведомлений — идеальный полигон для async. Никаких баз данных, только ASP.NET Core + Channel\<T\>.

| # | Задача | Что отрабатываем | Результат |
|---|---|---|---|
| 1 | Создать solution и структуру проектов | Организация кода, слои, project references | Репо готово |
| 2 | Минимальный API: создать задачу (in-memory) | ASP.NET Core minimal API, базовые модели | POST /tasks работает |
| 3 | NotificationService на Channel\<T\> | Channel\<T\>, producer-consumer, async pipeline | Очередь уведомлений |
| 4 | Реализовать отправку через BackgroundService | IHostedService, async/await без дедлоков | Фоновый воркер |
| 5 | Добавить CancellationToken везде | Graceful shutdown, propagation токена | Корректное завершение |
| 6 | Намеренно сломать дедлоком — починить | Воспроизвести .Result deadlock, разобраться | Понимание дедлока |
| 7 | Логирование через ILogger (структурное) | Structured logging, log levels, не строки | Читаемые логи |

- [x] 1. Создать solution и структуру проектов
- [ ] 2. Минимальный API: создать задачу (in-memory)
- [ ] 3. NotificationService на Channel\<T\>
- [ ] 4. Реализовать отправку через BackgroundService
- [ ] 5. Добавить CancellationToken везде
- [ ] 6. Намеренно сломать дедлоком — починить
- [ ] 7. Логирование через ILogger (структурное)

**Итог фазы:** работающий API с фоновым сервисом уведомлений. Всё запускается локально через `dotnet run`.

---

## 🐳 Фаза 2 — Тестирование + Docker
`Месяц 2–4 · Контейнеры и тесты появляются вместе`

Docker добавляется здесь осознанно: Testcontainers.NET позволяет поднимать реальную PostgreSQL прямо в тестах. С этой фазы весь проект живёт в контейнерах.

| # | Задача | Что отрабатываем | Результат |
|---|---|---|---|
| 8 | Dockerfile для Api (multi-stage) | multi-stage build, минимальный образ, .dockerignore | Образ < 200MB |
| 9 | docker-compose: Api + PostgreSQL + Redis | Сервисы, сети, volumes, env-переменные | Вся среда одной командой |
| 10 | Подключить EF Core, смигрировать схему | DbContext, миграции, connection string из env | БД работает в контейнере |
| 11 | Health checks в docker-compose | depends_on condition, /health endpoint | Сервисы стартуют в порядке |
| 12 | Первые unit-тесты доменных объектов | xUnit, AAA-паттерн, без зависимостей | 10+ unit-тестов |
| 13 | Integration-тест: POST /tasks через WebApplicationFactory | WebApplicationFactory, in-memory vs real DB | Первый integration-тест |
| 14 | Testcontainers: integration-тесты с реальной PostgreSQL | Testcontainers.NET, изоляция тестов | Тесты с реальной БД |
| 15 | Добавить RabbitMQ в docker-compose, подключить MassTransit | MassTransit, Consumer, Publisher | Уведомления через очередь |
| 16 | Тест: проверить публикацию события в RabbitMQ | Моки шины vs Testcontainers для RabbitMQ | Event flow покрыт тестом |

- [ ] 8. Dockerfile для Api (multi-stage)
- [ ] 9. docker-compose: Api + PostgreSQL + Redis
- [ ] 10. Подключить EF Core, смигрировать схему
- [ ] 11. Health checks в docker-compose
- [ ] 12. Первые unit-тесты доменных объектов
- [ ] 13. Integration-тест: POST /tasks через WebApplicationFactory
- [ ] 14. Testcontainers: integration-тесты с реальной PostgreSQL
- [ ] 15. Добавить RabbitMQ в docker-compose, подключить MassTransit
- [ ] 16. Тест: проверить публикацию события в RabbitMQ

**Итог фазы:** проект полностью контейнеризован, есть unit и integration тесты, уведомления идут через RabbitMQ.

---

## 🚀 Фаза 3 — CI/CD & Деплой
`Месяц 4–5 · Проект становится живым`

Цель: push в main → тесты → образ в GHCR → автодеплой на VPS. После этой фазы проект доступен по реальному домену.

| # | Задача | Что отрабатываем | Результат |
|---|---|---|---|
| 17 | CI workflow: build + test | GitHub Actions, checkout, setup-dotnet, кэш NuGet | Тесты запускаются на каждый PR |
| 18 | CD: сборка образа и push в GHCR | docker/build-push-action, тегирование по sha | Образ в реестре |
| 19 | Поднять VPS, настроить SSH-доступ | Hetzner/DO, SSH-ключи, ufw, non-root user | Сервер готов |
| 20 | nginx + SSL через Let's Encrypt | proxy_pass, certbot, автообновление сертификата | HTTPS по домену |
| 21 | Deploy workflow: SSH → docker pull → up | appleboy/ssh-action, GitHub Secrets | Автодеплой работает |
| 22 | Сканирование образа на уязвимости | trivy-action в pipeline | Security check в CI |
| 23 | Бейдж статуса pipeline в README | GitHub Actions badge | README с индикатором |

- [ ] 17. CI workflow: build + test
- [ ] 18. CD: сборка образа и push в GHCR
- [ ] 19. Поднять VPS, настроить SSH-доступ
- [ ] 20. nginx + SSL через Let's Encrypt
- [ ] 21. Deploy workflow: SSH → docker pull → up
- [ ] 22. Сканирование образа на уязвимости
- [ ] 23. Бейдж статуса pipeline в README

**Итог фазы:** `push в main` → тесты → образ в GHCR → сервис обновлён на VPS. Проект живёт по домену с HTTPS.

---

## 🏗️ Фаза 4 — Clean Architecture, CQRS, DDD
`Месяц 5–8 · Рефакторинг и проектирование домена`

Проект переосмысливается: код реструктурируется по принципам Clean Architecture, домен обогащается, CQRS становится явным.

| # | Задача | Что отрабатываем | Результат |
|---|---|---|---|
| 24 | Перенести логику в слои Domain / Application | Dependency Rule, инверсия зависимостей | Слои разделены |
| 25 | Выделить Aggregate: FreelanceTask | Aggregate Root, инварианты, приватные setters | Богатая доменная модель |
| 26 | Добавить Value Objects: Money, TaskStatus | Иммутабельность, equals по значению, валидация | VO вместо примитивов |
| 27 | Реализовать CreateTaskCommand через MediatR | Command, IRequestHandler, DI-регистрация | Первая команда |
| 28 | Реализовать GetTasksQuery с фильтрацией | Query, IRequestHandler, read-модель | Первый запрос |
| 29 | Pipeline Behavior: логирование всех команд | IPipelineBehavior, cross-cutting concerns | Автологирование |
| 30 | Pipeline Behavior: валидация через FluentValidation | ValidationBehavior, ошибки из пайплайна | Валидация централизована |
| 31 | Domain Event: TaskAccepted → уведомление | IDomainEvent, диспетчеризация после SaveChanges | Domain Events работают |
| 32 | Реализовать Bid (заявка) как отдельный Aggregate | Связи агрегатов через ID, не через навигацию | Второй агрегат |
| 33 | Написать ADR-001: почему Clean Architecture | Architecture Decision Record | docs/adr/001-clean-architecture.md |

- [ ] 24. Перенести логику в слои Domain / Application
- [ ] 25. Выделить Aggregate: FreelanceTask
- [ ] 26. Добавить Value Objects: Money, TaskStatus
- [ ] 27. Реализовать CreateTaskCommand через MediatR
- [ ] 28. Реализовать GetTasksQuery с фильтрацией
- [ ] 29. Pipeline Behavior: логирование всех команд
- [ ] 30. Pipeline Behavior: валидация через FluentValidation
- [ ] 31. Domain Event: TaskAccepted → уведомление
- [ ] 32. Реализовать Bid (заявка) как отдельный Aggregate
- [ ] 33. Написать ADR-001: почему Clean Architecture

**Итог фазы:** проект живёт по Clean Architecture, домен выражен через DDD-строительные блоки, CQRS реализован через MediatR.

---

## 🔍 Фаза 5 — SQL & производительность
`Месяц 8–10 · Оптимизация запросов и индексов`

Цель не "написать правильно", а "найти и исправить что сломано".

| # | Задача | Что отрабатываем | Результат |
|---|---|---|---|
| 34 | Включить EF Core query logging, найти N+1 | EF Core logging, LazyLoading ловушки | N+1 найден |
| 35 | Починить N+1 через Include и проекции | Eager loading, .Select() вместо полных сущностей | N+1 устранён |
| 36 | Добавить поиск задач по тексту и фильтры | LIKE vs индексы, составные условия | Поиск работает |
| 37 | Проанализировать EXPLAIN ANALYZE для 3 запросов | Execution plan, Seq Scan vs Index Scan | Планы изучены |
| 38 | Добавить индексы по горячим полям | Non-clustered, covering index, composite | Индексы добавлены |
| 39 | Реализовать keyset-пагинацию | Cursor vs offset, производительность | Быстрая пагинация |
| 40 | Добавить AsNoTracking в read-запросы | EF Core tracking overhead | Read-запросы оптимизированы |
| 41 | Сложный запрос через Dapper (статистика) | Dapper, raw SQL для аналитики | Гибридный подход |
| 42 | Кэшировать горячие запросы в Redis | IDistributedCache, сериализация, TTL | Кэш работает |

- [ ] 34. Включить EF Core query logging, найти N+1
- [ ] 35. Починить N+1 через Include и проекции
- [ ] 36. Добавить поиск задач по тексту и фильтры
- [ ] 37. Проанализировать EXPLAIN ANALYZE для 3 запросов
- [ ] 38. Добавить индексы по горячим полям
- [ ] 39. Реализовать keyset-пагинацию
- [ ] 40. Добавить AsNoTracking в read-запросы
- [ ] 41. Сложный запрос через Dapper (статистика)
- [ ] 42. Кэшировать горячие запросы в Redis

**Итог фазы:** запросы оптимизированы, индексы расставлены, есть понимание execution plan. Кэширование через Redis.

---

## 📊 Фаза 6 — ELK Stack + Observability
`Месяц 10–12 · Логи, метрики, трейсинг`

Три столпа observability добавляются последовательно. VPS уже есть — ELK поднимается там же.

| # | Задача | Что отрабатываем | Результат |
|---|---|---|---|
| 43 | Поднять ELK в docker-compose | Elasticsearch + Kibana + Filebeat | ELK стек запущен |
| 44 | Переключить Serilog на Elasticsearch sink | Structured logging, Elasticsearch sink | Логи летят в ES |
| 45 | Добавить enrichers: RequestId, UserId | Serilog enrichers, контекст в каждом логе | Логи с контекстом |
| 46 | Kibana Dashboard: ошибки и время ответа | KQL запросы, Discover, визуализации | Дашборд в Kibana |
| 47 | Добавить Prometheus + Grafana в docker-compose | prometheus-net, /metrics endpoint | Метрики экспортируются |
| 48 | Кастомные метрики: задачи, ошибки, время | Counter, Histogram, бизнес-метрики | Бизнес-метрики в Grafana |
| 49 | Grafana Dashboard: HTTP, БД, очередь | PromQL, алёрты на аномалии | Дашборд Grafana |
| 50 | Добавить OpenTelemetry трейсинг + Jaeger | OTel SDK, instrumentations, context propagation | Трейсинг работает |
| 51 | Проследить запрос: API → App → БД → очередь | Span, trace, корреляция с логами через TraceId | Сквозной трейс |

- [ ] 43. Поднять ELK в docker-compose
- [ ] 44. Переключить Serilog на Elasticsearch sink
- [ ] 45. Добавить enrichers: RequestId, UserId
- [ ] 46. Kibana Dashboard: ошибки и время ответа
- [ ] 47. Добавить Prometheus + Grafana в docker-compose
- [ ] 48. Кастомные метрики: задачи, ошибки, время
- [ ] 49. Grafana Dashboard: HTTP, БД, очередь
- [ ] 50. Добавить OpenTelemetry трейсинг + Jaeger
- [ ] 51. Проследить запрос: API → App → БД → очередь

**Итог фазы:** система полностью наблюдаема. Любую ошибку можно найти в Kibana, провалиться в трейс в Jaeger и посмотреть метрику в Grafana.

---

## 🎯 Фаза 7 — System Design & Tech Leadership
`Месяц 12–14 · Декомпозиция и архитектурные решения`

Монолит декомпозируется на сервисы. Фокус не только на коде, но и на документировании решений.

| # | Задача | Что отрабатываем | Результат |
|---|---|---|---|
| 52 | Выделить NotificationService в отдельный процесс | Физическое разделение, собственный Dockerfile | 2 сервиса в docker-compose |
| 53 | Реализовать Outbox Pattern | Outbox table, фоновый publisher, at-least-once | События не теряются |
| 54 | Добавить Polly: Retry + Circuit Breaker | Polly policies, resilience pipeline | Устойчивые вызовы |
| 55 | Написать ADR-002: почему Outbox | Обоснование паттерна, альтернативы, trade-offs | docs/adr/002-outbox.md |
| 56 | Написать ADR-003: монолит vs микросервисы | Когда декомпозировать, Modular Monolith | docs/adr/003-architecture.md |
| 57 | Добавить rate limiting в API (.NET 7+) | Rate limiting middleware, политики по endpoint | Rate limiting работает |
| 58 | Написать README с архитектурной схемой | C4 model (упрощённо), как запустить | Проект задокументирован |
| 59 | Провести self-review: что бы сделал иначе | Архитектурный разбор собственного кода | Список улучшений |

- [ ] 52. Выделить NotificationService в отдельный процесс
- [ ] 53. Реализовать Outbox Pattern
- [ ] 54. Добавить Polly: Retry + Circuit Breaker
- [ ] 55. Написать ADR-002: почему Outbox
- [ ] 56. Написать ADR-003: монолит vs микросервисы
- [ ] 57. Добавить rate limiting в API
- [ ] 58. Написать README с архитектурной схемой
- [ ] 59. Провести self-review: что бы сделал иначе

**Итог фазы:** полноценное портфолио. Проект можно показать на собеседовании и объяснить каждое архитектурное решение.

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