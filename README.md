# IncidentFlow

Enterprise-style **IT Incident Management System** built as a portfolio-ready full-stack application.

Employees report technical issues, technicians manage assignments, and administrators oversee operations, users, categories, and SLA compliance.

> **Note:** This project was originally developed **locally**. It has now been published on GitHub under [Mallek8](https://github.com/Mallek8) so the work can be versioned, shared, and presented as a portfolio repository.

> Status: **Phase 2 in progress / backend auth + persistence ready** — EF Core configurations, SQL Server migrations, Identity + JWT auth, and development seed data are implemented. Incident management APIs, React UI, Docker, and CI continue in later phases.

## Repository

- GitHub profile: [https://github.com/Mallek8](https://github.com/Mallek8)
- This codebase is the IncidentFlow portfolio project (local → GitHub).

## Technology stack

| Layer | Technologies |
| --- | --- |
| Backend | C#, ASP.NET Core Web API (.NET 9), EF Core, SQL Server, ASP.NET Core Identity, JWT + refresh tokens, FluentValidation, Swagger |
| Architecture | Clean Architecture (Domain, Application, Infrastructure, API, Tests) |
| Frontend | React, TypeScript, Vite, Tailwind CSS, TanStack Query, Recharts *(Phase 4+)* |
| Quality | xUnit, FluentAssertions, Moq |
| DevOps | Docker / Compose, GitHub Actions *(Phase 6)* |

## Solution structure

```
IncidentFlow.sln
├── src/
│   ├── IncidentFlow.Domain/           # Entities, enums, domain rules (no infra deps)
│   ├── IncidentFlow.Application/      # Use cases, DTOs, validation, interfaces
│   ├── IncidentFlow.Infrastructure/   # EF Core, Identity, JWT, seeding
│   └── IncidentFlow.API/              # HTTP endpoints, middleware, Swagger
├── tests/
│   └── IncidentFlow.Tests/            # Unit + integration tests
└── frontend/                          # React app (Phase 4)
```

### Dependency rule

`API → Application ← Infrastructure`  
`Application → Domain`  
`Infrastructure → Domain`  
Domain has **no** outward project dependencies.

## Features implemented so far

### Phase 1
- Clean Architecture solution scaffolding
- Domain entities, status transitions, SLA calculator
- Shared API response models and exception middleware
- Health endpoint + Swagger foundation

### Phase 2
- EF Core entity configurations, indexes, relationships
- Initial SQL Server migration (`InitialCreate`)
- ASP.NET Core Identity with roles: Admin, Technician, Employee
- JWT access tokens + rotatable refresh tokens
- Auth endpoints: register, login, refresh, logout, me
- Development seed data (users, categories, SLA policies, sample incident)

## Auth API (Phase 2)

| Method | Endpoint | Access |
| --- | --- | --- |
| POST | `/api/auth/register` | Anonymous (Employee only) |
| POST | `/api/auth/login` | Anonymous |
| POST | `/api/auth/refresh` | Anonymous |
| POST | `/api/auth/logout` | Authenticated |
| GET | `/api/auth/me` | Authenticated |
| GET | `/api/health` | Anonymous |

### Seeded development accounts

| Email | Password | Role |
| --- | --- | --- |
| `admin@incidentflow.local` | `Admin123!` | Admin |
| `tech1@incidentflow.local` | `Tech123!` | Technician |
| `tech2@incidentflow.local` | `Tech123!` | Technician |
| `employee@incidentflow.local` | `Employee123!` | Employee |

These are **demo-only** accounts with fictional data — not real personal information.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (local instance, Docker container, or Azure SQL)
- Node.js 20+ *(frontend phases)*
- Docker Desktop *(optional, Phase 6)*
- Git

## Run the API locally

```powershell
cd <repo-root>

# Optional: configure secrets
cd src/IncidentFlow.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=IncidentFlowDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
dotnet user-secrets set "Jwt:Key" "YOUR_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS"
cd ../..

dotnet restore
dotnet build
dotnet test
dotnet run --project src/IncidentFlow.API --launch-profile https
```

On Development startup the API attempts `Migrate()` + seed automatically.

- HTTP: `http://localhost:5073`
- HTTPS: `https://localhost:7269`
- Swagger: `https://localhost:7269/swagger`

Trust the HTTPS development certificate once:

```powershell
dotnet dev-certs https --trust
```

### Database migrations

```powershell
dotnet ef database update --project src/IncidentFlow.Infrastructure --startup-project src/IncidentFlow.API
```

Create a new migration later:

```powershell
dotnet ef migrations add <Name> --project src/IncidentFlow.Infrastructure --startup-project src/IncidentFlow.API --output-dir Persistence/Migrations
```

## Configuration

Sensitive values must come from environment variables or user secrets — do not commit real secrets.

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | SQL Server connection |
| `Jwt__Key` | JWT signing key (≥ 32 chars) |
| `Cors__AllowedOrigins__0` | Frontend origin |

See `.env.example` for a template.

## SLA calculation rules (demo)

Default resolution windows (calendar hours, 24/7 — **not** business-hours):

| Priority | Hours |
| --- | --- |
| Critical | 4 |
| High | 8 |
| Medium | 24 |
| Low | 72 |

`SlaDeadlineUtc = CreatedAtUtc + ResolutionTimeHours`. Overdue when `now` (or resolution time) is after the deadline. Business-hours calendars are a planned enhancement.

## Status transition rules

| From | Allowed next |
| --- | --- |
| New | Assigned, Closed |
| Assigned | In Progress, Closed |
| In Progress | Resolved, Assigned, Closed |
| Resolved | Closed, Reopened |
| Closed | Reopened |
| Reopened | Assigned, In Progress, Closed |

## Testing

```powershell
dotnet test
```

Current coverage includes domain status transitions, SLA helpers, and auth request validation.

## Screenshots

_Add UI screenshots after Phase 4–5._

## Roadmap

1. **Phase 1** — Solution + architecture foundation *(done)*
2. **Phase 2** — EF configurations, migrations, seed data, Identity + JWT *(done / pending live SQL Server verification)*
3. **Phase 3** — Incident management REST APIs and authorization
4. **Phase 4** — React frontend wired to the real API
5. **Phase 5** — Dashboards, role workflows, SLA monitoring
6. **Phase 6** — Broader tests, Docker Compose, GitHub Actions CI, docs polish

## Future improvements

- Business-hours SLA calendars and pause-on-waiting states
- Email / Teams notifications
- Attachment storage
- Audit export and advanced reporting
- Soft-delete and archival policies

## License

This project is intended for portfolio / demonstration use unless otherwise specified.
