# IncidentFlow

**IncidentFlow** is a full-stack IT incident management system.

Employees can report technical issues, technicians manage assigned incidents, and administrators oversee operations, users, categories, and SLA compliance.

**Created by Mallek Hannachi**

## Repository

- [https://github.com/Mallek8/IncidentFlow](https://github.com/Mallek8/IncidentFlow)

## Architecture and SOLID

The backend follows **Clean Architecture** and is intentionally shaped around **SOLID**, especially **Single Responsibility**.

```
API -> Application <- Infrastructure
            |
          Domain
```

| Principle | How IncidentFlow applies it |
| --- | --- |
| **S: Single Responsibility** | One reason to change per type: `IncidentService` handles incident lifecycle only; `CategoryService` and `DashboardService` are separate; `IncidentAccessPolicy` owns authorization rules; `IncidentMapper` owns DTO mapping; `TicketNumberGenerator` owns ticket numbers; `AuthService` vs `UserAdminService` split authentication from user administration. Controllers stay thin HTTP adapters. |
| **O: Open/Closed** | Status transitions and SLA rules live in Domain (`IncidentStatusTransitionRules`, `SlaCalculator`). New priorities/transitions extend those rules without rewriting controllers. |
| **L: Liskov Substitution** | Services are consumed through interfaces (`IIncidentService`, `IAuthService`, ...). Implementations can be replaced in tests (Moq / in-memory EF) without changing callers. |
| **I: Interface Segregation** | Clients depend on narrow contracts: incidents, categories, dashboard, auth, and user-admin are separate interfaces instead of one large "god" service. |
| **D: Dependency Inversion** | Application defines abstractions (`IApplicationDbContext`, `ICurrentUserService`, `IAuthService`, ...). Infrastructure implements them. API and Application never depend on concrete EF/Identity details. |

### Application feature layout

```
Application/
  Common/Interfaces/          # Abstractions (DIP)
  Features/
    Incidents/
      Services/               # Incident lifecycle
      Security/               # Access policy (SRP)
      Mapping/                # DTO mapping (SRP)
      Models/
      Validators/
    Categories/               # Category use-cases
    Dashboard/                # Analytics use-cases
    Auth/                     # Auth DTOs + validators
```

Domain keeps pure business rules (no EF, no HTTP). Infrastructure hosts EF Core, Identity, JWT, and seeding.

## Languages and technologies

| Area | Languages / tools |
| --- | --- |
| Backend | **C#**, ASP.NET Core Web API (.NET 9), EF Core, SQL Server / SQLite, Identity, JWT |
| Frontend | **TypeScript**, **CSS**/Tailwind, React, Vite |
| Infra / DevOps | **Dockerfile**, **YAML** (Docker Compose, GitHub Actions), Nginx |

## Technology stack

| Layer | Technologies |
| --- | --- |
| Backend | C#, ASP.NET Core Web API (.NET 9), EF Core, SQL Server, ASP.NET Core Identity, JWT + refresh tokens, FluentValidation, Swagger |
| Architecture | Clean Architecture + SOLID (Domain, Application, Infrastructure, API, Tests) |
| Frontend | React, TypeScript, Vite, Tailwind CSS, React Router, TanStack Query, Axios, Recharts, Lucide |
| Quality | xUnit, FluentAssertions, Moq |
| DevOps | Docker, Docker Compose, GitHub Actions CI |

## Solution structure

```
IncidentFlow.sln
  src/
    IncidentFlow.Domain/
    IncidentFlow.Application/
    IncidentFlow.Infrastructure/
    IncidentFlow.API/
  tests/IncidentFlow.Tests/
  frontend/
  scripts/
  docker-compose.yml
  .github/workflows/ci.yml
```

## Features

- Role-based access: Employee, Technician, Admin (API-enforced)
- Incident lifecycle with validated status transitions
- Comments (public + internal staff notes)
- Assignment workflow (Admin)
- SLA deadlines by priority and overdue monitoring
- Dashboard metrics with Recharts
- JWT authentication + refresh tokens
- Focused services for categories, dashboard, auth, and user admin
- Development seed data (SQLite locally; SQL Server via Docker)

## Auth API

| Method | Endpoint | Access |
| --- | --- | --- |
| POST | `/api/auth/register` | Anonymous (Employee only) |
| POST | `/api/auth/login` | Anonymous |
| POST | `/api/auth/refresh` | Anonymous |
| POST | `/api/auth/logout` | Authenticated |
| GET | `/api/auth/me` | Authenticated |

## Incident API

| Method | Endpoint | Access |
| --- | --- | --- |
| GET | `/api/incidents` | Authenticated (scoped by role) |
| GET | `/api/incidents/{id}` | Authenticated (scoped by role) |
| POST | `/api/incidents` | Authenticated |
| PATCH | `/api/incidents/{id}/status` | Technician / Admin |
| POST | `/api/incidents/{id}/assign` | Admin |
| POST | `/api/incidents/{id}/comments` | Authenticated |
| POST | `/api/incidents/{id}/reopen` | Reporter / Admin |
| GET | `/api/categories` | Authenticated |
| POST | `/api/categories` | Admin |
| GET | `/api/dashboard/summary` | Authenticated |
| GET | `/api/users` | Admin |
| PUT | `/api/users/{id}/roles` | Admin |
| PUT | `/api/users/{id}/active` | Admin |

### Seeded development accounts

| Email | Password | Role |
| --- | --- | --- |
| `admin@incidentflow.local` | `Admin123!` | Admin |
| `tech1@incidentflow.local` | `Tech123!` | Technician |
| `tech2@incidentflow.local` | `Tech123!` | Technician |
| `employee@incidentflow.local` | `Employee123!` | Employee |

Development seed accounts use fictional data only.

## Prerequisites

- .NET 9 SDK
- Node.js 20+
- SQL Server optional locally (SQLite is used in Development)
- Docker Desktop (optional)
- Git

## Run locally

Development uses **SQLite** by default (`appsettings.Development.json`). Docker Compose still uses SQL Server.

Run **both** API and frontend.

### Backend

```powershell
cd <repo-root>
dotnet restore
dotnet build
dotnet test
dotnet run --project src/IncidentFlow.API --launch-profile http
# or: .\scripts\start-api.ps1
```

- Swagger: `http://localhost:5088/swagger`
- HTTP API: `http://localhost:5088`

### Frontend

```powershell
cd frontend
npm install
npm run dev
# or: .\scripts\start-frontend.ps1
```

- UI: `http://localhost:5188`
- Dev proxy: `/api` -> `http://localhost:5088`

### Docker Compose

```powershell
docker compose up --build
```

- API: `http://localhost:8080`
- Web: `http://localhost:8081`
- SQL Server: `localhost:1433`

## SLA calculation rules

| Priority | Hours (calendar, 24/7) |
| --- | --- |
| Critical | 4 |
| High | 8 |
| Medium | 24 |
| Low | 72 |

`SlaDeadlineUtc = CreatedAtUtc + ResolutionTimeHours`.

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
cd frontend
npm run build
```

Tests cover domain rules (transitions, SLA), FluentValidation, incident access policy, and invalid status transitions via `IncidentService`.

## Screenshots

_Add UI screenshots as needed._

## Roadmap

- Business-hours SLA calendars
- Attachments and notifications
- Expanded integration tests
- Soft-delete / archival policies

## License

All rights reserved unless otherwise specified.
