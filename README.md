# Lienzo — Classroom Reservation & Management System

A production-ready, full-stack web application for university classroom reservation and management, built with Clean Architecture principles.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Lienzo.Web                           │
│              React + TypeScript + Vite + TailwindCSS        │
│                    shadcn/ui + TanStack Query               │
├─────────────────────────────────────────────────────────────┤
│                        Lienzo.API                           │
│           ASP.NET Core 9 Web API + SignalR + Swagger        │
├─────────────────────────────────────────────────────────────┤
│                    Lienzo.Application                        │
│        CQRS (MediatR) · AutoMapper · FluentValidation       │
│            Commands · Queries · DTOs · Interfaces            │
├─────────────────────────────────────────────────────────────┤
│                    Lienzo.Infrastructure                      │
│       EF Core 9 · PostgreSQL (Npgsql) · ASP.NET Identity     │
│      JWT Bearer · Repository + Unit of Work · Serilog       │
├─────────────────────────────────────────────────────────────┤
│                      Lienzo.Domain                           │
│  Entities · Value Objects · Enums · Events · Interfaces     │
│            Rich domain model (zero external deps)            │
└─────────────────────────────────────────────────────────────┘
```

### Clean Architecture Dependency Rule

**Dependencies point inward.** Domain has zero external references. Application depends on Domain. Infrastructure depends on Application. API depends on Infrastructure.

## Tech Stack

### Backend
| Layer | Technology |
|-------|-----------|
| Runtime | .NET 9 / ASP.NET Core 9 |
| CQRS | MediatR 12 |
| ORM | Entity Framework Core 9 + Npgsql |
| Database | PostgreSQL 16 |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Logging | Serilog |
| Real-time | SignalR |
| API Docs | Swagger / OpenAPI |

### Frontend
| Technology | Purpose |
|-----------|---------|
| React 18 + TypeScript | UI framework |
| Vite | Build tool |
| TailwindCSS | Styling |
| shadcn/ui | Component library |
| React Router v6 | Routing |
| TanStack Query | Server state |
| Zustand | Client state |
| React Hook Form + Zod | Forms + validation |
| SignalR | Real-time notifications |
| Recharts | Charts / analytics |
| Lucide React | Icons |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [PostgreSQL 16](https://www.postgresql.org/download/) (if running without Docker)

## Quick Start (Docker)

```bash
# Clone and start all services
git clone <repo-url> && cd Lienzo
docker compose up --build
```

This starts:
- **PostgreSQL** on port `5432`
- **pgAdmin** on port `5050`
- **Lienzo API** on port `5000`
- **Lienzo Web** on port `3000`

Visit `http://localhost:3000` to use the application.

### Default Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@lienzo.edu | Admin123! |
| Teacher | teacher1@lienzo.edu | Teacher123! |
| Teacher | teacher2@lienzo.edu | Teacher123! |
| Teacher | teacher3@lienzo.edu | Teacher123! |
| Student | student1@lienzo.edu | Student123! |
| Student | student2-10@lienzo.edu | Student123! |

## Local Development (without Docker)

### Backend

```bash
# Set up the database
# Ensure PostgreSQL is running and create a database named 'lienzo'

# Navigate to API project
cd src/Lienzo.API

# Run with auto-migration and seed
dotnet run
```

The API will be available at `http://localhost:5000` with Swagger at `http://localhost:5000/swagger`.

### Frontend

```bash
cd src/Lienzo.Web
npm install
npm run dev
```

The frontend will be available at `http://localhost:5173` with API proxy to `http://localhost:5000`.

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=lienzo;Username=lienzo;Password=lienzo123` |
| `JwtSettings__Secret` | JWT signing key | `LienzoSuperSecretKeyForJwtTokenGeneration2024!` |
| `JwtSettings__Issuer` | JWT issuer | `LienzoAPI` |
| `JwtSettings__Audience` | JWT audience | `LienzoClient` |
| `JwtSettings__ExpirationInMinutes` | Access token expiry | `60` |
| `JwtSettings__RefreshTokenExpirationInDays` | Refresh token expiry | `7` |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |

## API Overview

### Authentication

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/login` | No | Login with email/password |
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/refresh-token` | No | Refresh JWT token |
| GET | `/api/auth/me` | Yes | Get current user |

### Buildings

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/buildings` | Yes | List all buildings |
| POST | `/api/buildings` | Admin | Create building |
| GET | `/api/buildings/{id}` | Yes | Get building detail |
| PUT | `/api/buildings/{id}` | Admin | Update building |
| DELETE | `/api/buildings/{id}` | Admin | Soft delete building |
| GET | `/api/buildings/{id}/classrooms` | Yes | Get building classrooms |

### Classrooms

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/classrooms` | Yes | List with filters |
| POST | `/api/classrooms` | Admin | Create classroom |
| GET | `/api/classrooms/{id}` | Yes | Get classroom detail |
| PUT | `/api/classrooms/{id}` | Admin | Update classroom |
| DELETE | `/api/classrooms/{id}` | Admin | Soft delete classroom |
| GET | `/api/classrooms/{id}/availability` | Yes | Check time slot availability |

### Reservations

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/reservations` | Yes | List (paginated, role-filtered) |
| POST | `/api/reservations` | Teacher/Student | Create reservation |
| GET | `/api/reservations/{id}` | Yes | Get reservation detail |
| PUT | `/api/reservations/{id}` | Owner/Admin | Update reservation |
| DELETE | `/api/reservations/{id}` | Owner/Admin | Cancel reservation |
| PATCH | `/api/reservations/{id}/approve` | Admin | Approve reservation |
| PATCH | `/api/reservations/{id}/reject` | Admin | Reject reservation |
| PATCH | `/api/reservations/{id}/cancel` | Owner/Admin | Cancel reservation |

### Announcements

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/announcements` | Teacher/Admin | Create announcement |
| GET | `/api/announcements` | Yes | List announcements |
| GET | `/api/announcements/{id}` | Yes | Get announcement detail |
| PATCH | `/api/announcements/{id}/read` | Student | Mark as read |

### Notifications

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/notifications` | Yes | List user notifications |
| PATCH | `/api/notifications/{id}/read` | Yes | Mark notification as read |
| PATCH | `/api/notifications/read-all` | Yes | Mark all as read |

### Dashboard (Admin)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/dashboard/stats` | Admin | Platform statistics |
| GET | `/api/dashboard/occupancy-heatmap` | Admin | Occupancy heatmap data |

### Real-Time (SignalR)

| Hub | Events | Description |
|-----|--------|-------------|
| `/hubs/notifications` | `ReceiveNotification` | New notifications |
| | `ReservationStatusChanged` | Reservation approved/rejected |
| | `NewAnnouncement` | New announcement published |

## Project Structure

```
Lienzo/
├── src/
│   ├── Lienzo.Domain/           # Entities, value objects, enums, events, interfaces
│   ├── Lienzo.Application/      # CQRS commands/queries, DTOs, validators, mapping
│   ├── Lienzo.Infrastructure/   # EF Core, repositories, Identity, JWT, services
│   ├── Lienzo.API/              # Controllers, middleware, SignalR hub, Program.cs
│   └── Lienzo.Web/              # React + TypeScript + Vite frontend
├── tests/
│   ├── Lienzo.Domain.Tests/     # Entity behavior tests (44 tests)
│   ├── Lienzo.Application.Tests/# Command handler tests (10 tests)
│   └── Lienzo.API.Tests/        # API integration tests
├── docker-compose.yml           # Orchestration for all services
├── .env.example                 # Environment variable template
└── README.md
```

## Testing

```bash
# Run all backend tests
dotnet test

# Run specific test project
dotnet test tests/Lienzo.Domain.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Currently **44+ tests** covering:
- Domain: Reservation conflict logic, entity state transitions, domain events
- Application: Command handler validation, conflict detection, authorization
- Value Objects: TimeRange overlap detection

## Key Design Decisions

- **Rich Domain Model**: Entities encapsulate behavior and enforce invariants (e.g., `Reservation.Approve()` validates status before transitioning)
- **Domain Events**: Status changes publish events consumed by application handlers for notification dispatch
- **CQRS**: Every API action is a command or query with its own handler, enabling clear separation of concerns
- **Conflict Detection**: Double-booking prevented at both domain level (`IsAvailable`) and application level (`HasConflictAsync`)
- **Soft Deletes**: All entities support soft delete with global query filters
- **Audit Trail**: `CreatedAt`/`UpdatedAt` auto-set via EF Core `SaveChanges` override

## Database Schema

| Table | Description |
|-------|-------------|
| `edificios` | Buildings with floors |
| `aulas` | Classrooms with type, capacity, features (JSONB) |
| `AspNetUsers` | Extended Identity users |
| `reservas` | Reservations with status workflow |
| `comunicados` | Announcements (cancellation, postponement, general) |
| `comunicado_destinatarios` | Announcement-student junction table |
| `notificaciones` | User notifications |

## PWA Support

Lienzo Web is PWA-ready with:
- `manifest.json` with app name, icons, theme color
- Mobile-first responsive design (320px minimum)
- Bottom navigation on mobile
- Touch-friendly 44px tap targets
- Service worker for offline shell caching (add `sw.ts` for full offline support)

## License

MIT
# Lienzo
