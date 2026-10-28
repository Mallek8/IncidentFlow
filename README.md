# IncidentFlow

**IncidentFlow** is a full-stack IT incident management system.

Employees can report technical issues, technicians manage assigned incidents, and administrators oversee operations, users, categories, and SLA compliance.

## Repository

- [https://github.com/Mallek8/IncidentFlow](https://github.com/Mallek8/IncidentFlow)

## Languages & technologies

| Area | Languages / tools |
| --- | --- |
| Backend | **C#**, ASP.NET Core Web API (.NET 9), EF Core, SQL Server, Identity, JWT |
| Frontend | **TypeScript**, **JavaScript**, **CSS**/Tailwind, React, Vite |
| Infra / DevOps | **Dockerfile**, **YAML** (Docker Compose, GitHub Actions), **Nginx** config |
| Data | SQL Server (T-SQL via EF Core migrations) |

GitHub’s language bar reflects tracked source files (mainly C#, TypeScript, CSS, and YAML once indexed).

## Technology stack

| Layer | Technologies |
| --- | --- |
| Backend | C#, ASP.NET Core Web API (.NET 9), EF Core, SQL Server, ASP.NET Core Identity, JWT + refresh tokens, FluentValidation, Swagger |
| Architecture | Clean Architecture (Domain, Application, Infrastructure, API, Tests) |
| Frontend | React, TypeScript, Vite, Tailwind CSS, React Router, TanStack Query, Axios, Recharts, Lucide |
| Quality | xUnit, FluentAssertions, Moq |
| DevOps | Docker, Docker Compose, GitHub Actions CI |

## Solution structure

```
IncidentFlow.sln
├── src/
│   ├── IncidentFlow.Domain/
│   ├── IncidentFlow.Application/
│   ├── IncidentFlow.Infrastructure/
│   └── IncidentFlow.API/
├── tests/IncidentFlow.Tests/
├── frontend/                 # React + TypeScript UI
├── docker-compose.yml
└── .github/workflows/ci.yml
```

## Features

- Role-based access: Employee, Technician, Admin (enforced on the API)
- Incident lifecycle with validated status transitions
- Comments (public + internal staff notes)
- Assignment workflow (Admin)
- SLA deadlines by priority and overdue monitoring
- Dashboard metrics with Recharts
- JWT authentication + refresh tokens
- EF Core migrations and development seed data

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
- SQL Server (local, Docker, or Azure SQL)
- Docker Desktop (optional)
- Git

## Run locally

### Backend

```powershell
cd <repo-root>
dotnet restore
dotnet build
dotnet test
dotnet run --project src/IncidentFlow.API --launch-profile https
```

- Swagger: `https://localhost:7269/swagger`
- HTTP API: `http://localhost:5073`

Configure SQL Server and JWT via user secrets or environment variables (see `.env.example`).

### Frontend

```powershell
cd frontend
npm install
npm run dev
```

- UI: `http://localhost:5173`
- Set `VITE_API_BASE_URL=http://localhost:5073` if needed

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

## Screenshots

_Add UI screenshots as needed._

## Roadmap

- Broader admin user-management APIs
- Business-hours SLA calendars
- Attachments and notifications
- Expanded integration tests

## License

All rights reserved unless otherwise specified.
