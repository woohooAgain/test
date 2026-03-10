# 📚 План прокачки хардскиллов
**.NET · Middle → Lead/Architect · 14 месяцев · 30–60 минут в день**

---

## Оценка текущего уровня

| Область | Уровень | Комментарий |
|---|---|---|
| Практика (.NET стек, ASP.NET, EF, Redis, MQ) | 🟢 Уверенный Middle | Богатый опыт, широкий стек |
| CQRS | 🟡 Знает суть | Понимает разделение C/Q, MediatR, репликацию |
| Clean Architecture | 🟠 Пробел | Путает с N-Tier, не знает про Dependency Rule |
| DDD | 🟠 Концептуально | Понял идею, строительные блоки не знает |
| Async/Await (глубина) | 🔴 Пробел | Не знает deadlock при .Result, SynchronizationContext |
| Тестирование | 🔴 Пробел | Сам назвал слабым местом |
| SQL / индексы | 🔴 Пробел | Сам назвал слабым местом |
| Docker | 🔴 Не оценивалось | Добавлено в план |
| CI/CD & Деплой | 🔴 Не оценивалось | Добавлено в план |
| ELK Stack | 🔴 Не оценивалось | Добавлено в план |

**Вывод:** сильная практическая база, но отсутствие теоретического фундамента создаёт потолок роста. Именно этот разрыв мешает переходу на Senior и выше.

---

## Дорожная карта

| Фаза | Тема | Срок | Ожидаемый результат |
|---|---|---|---|
| Фаза 1 | Async/await & многопоточность | Месяц 1–2 | Уверенное понимание Task, deadlock, ConfigureAwait, ThreadPool |
| Фаза 2 | Тестирование + Docker | Месяц 2–4 | Unit/integration тесты, Testcontainers, вся среда в docker-compose |
| Фаза 3 | CI/CD & Деплой | Месяц 4–5 | GitHub Actions pipeline, образы в GHCR, живой деплой на VPS |
| Фаза 4 | Архитектура (Clean, CQRS, DDD) | Месяц 5–8 | Проектирует слои, знает DDD-блоки, применяет на практике |
| Фаза 5 | SQL & производительность | Месяц 8–10 | Execution plan, индексы, N+1, оптимизация EF Core |
| Фаза 6 | ELK + Observability | Месяц 10–12 | Полный стек наблюдаемости: логи, метрики, трейсинг |
| Фаза 7 | System Design & Leadership | Месяц 12–14 | Проектирует системы, ведёт arch-дискуссии, менторит |

> **Репозиторий:** GitHub (публичный). Бесплатные Actions минуты, работодатели могут смотреть код.

---

## Фаза 1 — Async/Await & многопоточность
`Месяц 1–2`

Фундамент всего современного .NET-бэкенда. Без этого невозможно разобраться в проблемах производительности и поведении ASP.NET Core под нагрузкой.

**Что изучить:**
- [ ] Task, Task\<T\>, ValueTask — разница и когда что использовать
- [ ] ThreadPool: как .NET управляет потоками, work-stealing
- [ ] async/await под капотом: state machine, continuation, как компилятор разворачивает код
- [ ] SynchronizationContext: что это, как ведёт себя в ASP.NET Core (его нет!) vs WinForms/WPF
- [ ] ConfigureAwait(false): когда нужен, а когда нет и почему
- [ ] Deadlock при .Result / .Wait() — механизм и как избегать
- [ ] CancellationToken: правильные паттерны использования
- [ ] Параллелизм vs конкурентность: Parallel.ForEach, PLINQ, Channel\<T\>

**Практика:**
- [ ] Воспроизвести deadlock с .Result, зафиксировать, починить
- [ ] Написать producer-consumer на Channel\<T\>
- [ ] Найти в рабочем коде места с .Result/.Wait() и оценить риски

**Ресурсы:**
- Stephen Cleary — блог "There Is No Thread" (обязательно)
- Книга: "Concurrency in C# Cookbook" — Stephen Cleary
- Microsoft Docs: Task-based Asynchronous Pattern (TAP)

---

## Фаза 2 — Тестирование + Docker
`Месяц 2–4`

Две темы вместе: Docker нужен для нормального integration-тестирования через Testcontainers.NET. С этой фазы весь проект живёт в контейнерах.

**Тестирование — что изучить:**
- [ ] Пирамида тестирования: unit → integration → e2e, что и когда тестировать
- [ ] xUnit: синтаксис, атрибуты, жизненный цикл тестов
- [ ] Моки: Moq или NSubstitute — Setup, Verify, когда мокать, а когда нет
- [ ] Тестирование ASP.NET Core: WebApplicationFactory, TestServer
- [ ] AAA-паттерн: Arrange, Act, Assert — структура читаемых тестов
- [ ] Testability: DI, интерфейсы, избегание static/new внутри методов
- [ ] Testcontainers.NET: поднимать PostgreSQL / Redis / RabbitMQ прямо в тестах

**Docker — что изучить:**
- [ ] Основы: образ, контейнер, слои, Dockerfile — как это работает
- [ ] Написание Dockerfile для .NET: multi-stage build, минимальный образ
- [ ] docker-compose: поднимать локальную среду одной командой
- [ ] Volumes, networks, health checks, depends_on condition
- [ ] docker-compose override: разные конфиги для dev / test

**Практика:**
- [ ] Dockerfile для .NET API с multi-stage build
- [ ] docker-compose: API + PostgreSQL + Redis + RabbitMQ
- [ ] Integration-тест через Testcontainers.NET с реальной PostgreSQL

**Ресурсы:**
- Книга: "Unit Testing Principles, Practices, and Patterns" — Vladimir Khorikov
- docs.docker.com — Getting Started
- GitHub: testcontainers/testcontainers-dotnet

---

## Фаза 3 — CI/CD & Деплой
`Месяц 4–5`

CI/CD идёт сразу после Docker — Docker уже знаком, а GitHub Actions активно его использует. Цель фазы: push в main → тесты → образ в реестре → автодеплой на VPS. Проект становится живым.

**GitHub Actions — что изучить:**
- [ ] Концепция: workflow, job, step, runner — как это устроено
- [ ] Триггеры: on push, on pull_request, on release — когда что запускать
- [ ] CI pipeline: checkout → setup .NET → restore → build → test
- [ ] Кэширование зависимостей: actions/cache для NuGet пакетов
- [ ] Secrets и переменные окружения: как передавать чувствительные данные
- [ ] Артефакты: сохранять test results, покрытие кода
- [ ] Статус бейджи в README

**Docker Hub / GHCR — что изучить:**
- [ ] GHCR (GitHub Container Registry) vs Docker Hub — что выбрать и почему
- [ ] Публикация образа: docker build → docker tag → docker push из Actions
- [ ] Тегирование образов: latest, sha коммита, семантическое версионирование
- [ ] Сканирование образов на уязвимости: trivy или встроенный Dependabot

**Деплой на VPS — что изучить:**
- [ ] Выбор VPS: Hetzner, DigitalOcean, Timeweb — достаточно самого дешёвого (2–4$/мес)
- [ ] Первичная настройка: SSH-ключи, firewall (ufw), non-root пользователь
- [ ] nginx как reverse proxy: proxy_pass, SSL через Let's Encrypt (certbot)
- [ ] docker-compose на сервере: production compose файл
- [ ] Стратегия деплоя: SSH в Actions → docker pull → docker-compose up -d
- [ ] Secrets на сервере: .env файл вне репозитория, передача через GitHub Secrets

**Итоговый pipeline:**
```
push в main
  → GitHub Actions: тесты
  → сборка Docker образа → push в GHCR
  → SSH на VPS → docker pull → docker-compose up -d
  → nginx раздаёт по домену с HTTPS
```

**Практика:**
- [ ] Написать CI workflow: build + test с кэшированием NuGet
- [ ] Добавить CD: сборка образа и push в GHCR при merge в main
- [ ] Поднять VPS, настроить nginx + certbot
- [ ] Написать deploy workflow: SSH + docker-compose pull + up
- [ ] Добавить бейдж статуса пайплайна в README

**Ресурсы:**
- GitHub Docs: docs.github.com/actions
- nginx docs: nginx.org/en/docs
- Let's Encrypt: certbot.eff.org

---

## Фаза 4 — Архитектура: Clean Architecture, CQRS, DDD
`Месяц 5–8`

Самая объёмная фаза — качественный сдвиг от разработчика к архитектору. По месяцу на каждую тему.

**Clean Architecture (месяц 5):**
- [ ] Dependency Rule: зависимости направлены строго внутрь, к домену
- [ ] Структура проектов: Domain, Application, Infrastructure, Presentation
- [ ] Интерфейсы в Application, реализации в Infrastructure — инверсия зависимостей
- [ ] Vs N-Tier: чётко сформулировать разницу, уметь объяснить на собеседовании

**CQRS углублённо (месяц 5–6):**
- [ ] MediatR Pipeline Behaviors — логирование, валидация, обработка ошибок
- [ ] Command validation с FluentValidation
- [ ] Read-модели: проекции, denormalization для быстрых query
- [ ] Event Sourcing как дополнение к CQRS (концептуально)

**DDD (месяц 6–7):**
- [ ] Entity vs Value Object: идентичность, иммутабельность VO
- [ ] Aggregate Root: инварианты, консистентность, размер агрегата
- [ ] Domain Events vs Integration Events
- [ ] Repository pattern в DDD-контексте
- [ ] Ubiquitous Language: единый язык с бизнесом
- [ ] Анемичная модель vs богатая доменная модель — рефакторинг

**Практика:**
- [ ] Pet-проект с нуля: Clean Architecture + CQRS + DDD (деплоится через Actions)
- [ ] Рефакторинг модуля рабочего проекта по принципам Clean Architecture

**Ресурсы:**
- Книга: "Domain-Driven Design" — Eric Evans (хотя бы первую часть)
- Книга: "Implementing Domain-Driven Design" — Vaughn Vernon
- GitHub: ardalis/CleanArchitecture, jasontaylordev/CleanArchitecture

---

## Фаза 5 — SQL & производительность
`Месяц 8–10`

Работая с EF Core долго, легко не замечать проблемы на уровне БД. Цель — найти и исправить что сломано в реальном проекте.

**Что изучить:**
- [ ] Execution plan: Index Scan vs Index Seek vs Table Scan, как читать
- [ ] Индексы: clustered vs non-clustered, covering, composite — когда не помогает
- [ ] N+1 проблема: найти через логирование, починить через Include и проекции
- [ ] EF Core: AsNoTracking, Split Queries, compiled queries, Dapper для сложных запросов
- [ ] Транзакции: уровни изоляции и их влияние на поведение под нагрузкой
- [ ] Пагинация: offset vs keyset (cursor) — производительность на больших данных
- [ ] Redis: паттерны ключей, TTL, eviction политики

**Практика:**
- [ ] Включить EF Core query logging, найти N+1 в рабочем проекте
- [ ] Проанализировать execution plan для 3–5 самых тяжёлых запросов
- [ ] Добавить missing indexes, измерить результат до и после

---

## Фаза 6 — ELK Stack + Observability
`Месяц 10–12`

Три столпа observability: логи → метрики → трейсинг. VPS уже есть — ELK можно поднять там же.

**ELK Stack:**
- [ ] Elasticsearch: индекс, шарды, реплики, полнотекстовый поиск
- [ ] Structured logging в .NET: Serilog, enrichers, Elasticsearch sink
- [ ] Kibana: Discover, Dashboard, KQL запросы, алёрты
- [ ] Filebeat: сбор логов из stdout контейнеров

**Метрики — Prometheus + Grafana:**
- [ ] Counter, Gauge, Histogram — типы метрик и когда что использовать
- [ ] prometheus-net: экспозиция метрик из ASP.NET Core
- [ ] Grafana: дашборды, PromQL, алёрты на аномалии

**Трейсинг — OpenTelemetry:**
- [ ] Distributed tracing: span, trace, context propagation между сервисами
- [ ] OpenTelemetry .NET SDK: инструментация ASP.NET Core, HttpClient, EF Core
- [ ] Jaeger как backend для трейсов, корреляция с логами через TraceId

**Практика:**
- [ ] Поднять ELK в docker-compose, подключить Serilog → Elasticsearch
- [ ] Создать Kibana Dashboard: ошибки, время ответа, топ endpoint
- [ ] Подключить Prometheus + Grafana, кастомные бизнес-метрики
- [ ] OpenTelemetry трейсинг: проследить запрос от API до БД до очереди

**Ресурсы:**
- elastic.co/guide
- opentelemetry.io/docs/languages/dotnet
- grafana.com/docs

---

## Фаза 7 — System Design & Tech Leadership
`Месяц 12–14`

Переход от Senior к Lead/Architect. Технические решения неотделимы от коммуникации и менторства.

**System Design:**
- [ ] Паттерны надёжности: Circuit Breaker, Retry, Bulkhead (Polly в .NET)
- [ ] Event-driven: Outbox pattern, saga, distributed transactions
- [ ] Kubernetes: Pod, Deployment, Service, Ingress — от docker-compose к k8s
- [ ] API Gateway, rate limiting, аутентификация на уровне инфраструктуры

**Tech Leadership:**
- [ ] ADR (Architecture Decision Records): документировать решения и trade-offs
- [ ] Code review с фокусом на архитектуру, не только стиль
- [ ] Менторство: объяснять паттерны, давать задачи с контекстом
- [ ] Писать RFC/tech proposals для новых фич

**Ресурсы:**
- Книга: "Designing Data-Intensive Applications" — Martin Kleppmann
- Книга: "Building Microservices" — Sam Newman

---

## Контрольные точки

После каждой фазы — честный вопрос себе:

- [ ] **Async/await:** могу объяснить deadlock, SynchronizationContext, зачем ConfigureAwait(false)?
- [ ] **Тестирование:** есть проект с unit + integration тестами через Testcontainers?
- [ ] **Docker:** могу написать Dockerfile, docker-compose с несколькими сервисами и health check?
- [ ] **CI/CD:** push в main → тесты → образ в GHCR → деплой на VPS — всё автоматически?
- [ ] **nginx:** умею настроить reverse proxy с HTTPS через Let's Encrypt?
- [ ] **Clean Architecture:** могу нарисовать диаграмму зависимостей и объяснить каждый слой?
- [ ] **DDD:** могу назвать и объяснить Entity, VO, Aggregate, Domain Event на примере?
- [ ] **SQL:** нашёл и исправил N+1 и missing index в реальном проекте?
- [ ] **ELK:** могу найти конкретную ошибку по логам, провалиться в трейс, открыть дашборд?
- [ ] **System Design:** могу провести whiteboard-сессию по архитектуре нового сервиса?

---

## Принципы

- **Практика важнее теории.** Читать без кода — почти бесполезно. Каждая тема заканчивается написанным кодом.
- **Проект живёт в продакшне.** После фазы 3 pet-проект деплоится на реальный VPS. Мотивирует и даёт практику эксплуатации.
- **GitHub как портфолио.** Публичный репо, зелёные бейджи CI, красивый README — это уже часть резюме.
- **Один рабочий проект как полигон.** Постепенно применять изученное там — лучший способ закрепить.
- **Не прыгать между темами.** Закончить фазу — перейти к следующей. Глубина важнее ширины.
- **Собеседования как метрика.** Раз в 3–4 месяца — техническое собеседование. Лучший тест реального уровня.

---

*Главное — последовательность, а не скорость.*