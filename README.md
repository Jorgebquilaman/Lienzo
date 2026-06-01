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

## Deployment on Debian

### 1. Server Prerequisites

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET 9 SDK
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
sudo ./dotnet-install.sh --channel 9.0 --install-dir /usr/share/dotnet
sudo ln -sf /usr/share/dotnet/dotnet /usr/local/bin/dotnet
dotnet --version  # Verify

# Install Node.js 20+
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo bash -
sudo apt install -y nodejs
node --version   # Verify

# Install PostgreSQL 16
sudo apt install -y postgresql-16 postgresql-client-16
sudo systemctl enable --now postgresql

# Install Nginx
sudo apt install -y nginx
sudo systemctl enable --now nginx

# Install build tools
sudo apt install -y git curl gnupg build-essential
```

### 2. Database Setup

```bash
# Create database and user
sudo -u postgres psql -c "CREATE USER lienzo WITH PASSWORD 'TuPasswordSegura';"
sudo -u postgres psql -c "CREATE DATABASE lienzo OWNER lienzo;"
sudo -u postgres psql -c "ALTER USER lienzo CREATEDB;"
```

### 3. Clone and Configure the Application

```bash
# Clone the repository
cd /opt
sudo git clone https://github.com/Jorgebquilaman/Lienzo.git
sudo chown -R $USER:$USER Lienzo
cd Lienzo

# Create production appsettings
cat > src/Lienzo.API/appsettings.Production.json << 'EOF'
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=lienzo;Username=lienzo;Password=TuPasswordSegura"
  },
  "JwtSettings": {
    "Secret": "UnSecretKeySeguroYLargoDeAlMenos32Caracteres!",
    "Issuer": "LienzoAPI",
    "Audience": "LienzoClient",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  },
  "Serilog": {
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/lienzo/api-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
EOF

# Build backend
cd src/Lienzo.API
dotnet publish -c Release -o /opt/Lienzo/publish

# Apply migrations and seed data
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://0.0.0.0:5002"
dotnet /opt/Lienzo/publish/Lienzo.API.dll --seed
```

### 4. Systemd Service for the API

```bash
sudo tee /etc/systemd/system/lienzo-api.service > /dev/null << 'EOF'
[Unit]
Description=Lienzo Classroom Reservation API
After=network.target postgresql.service

[Service]
Type=simple
User=www-data
WorkingDirectory=/opt/Lienzo/publish
ExecStart=/usr/share/dotnet/dotnet /opt/Lienzo/publish/Lienzo.API.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5002

[Install]
WantedBy=multi-user.target
EOF

# Create log directory
sudo mkdir -p /var/log/lienzo
sudo chown www-data:www-data /var/log/lienzo

# Start service
sudo systemctl daemon-reload
sudo systemctl enable --now lienzo-api
sudo systemctl status lienzo-api  # Verify
```

### 5. Build and Deploy the Frontend

```bash
# Build frontend
cd /opt/Lienzo/src/Lienzo.Web
npm install
npm run build

# Copy to web root
sudo rm -rf /var/www/lienzo
sudo cp -r dist /var/www/lienzo
sudo chown -R www-data:www-data /var/www/lienzo
```

### 6. Configure Nginx as Reverse Proxy

```bash
sudo tee /etc/nginx/sites-available/lienzo > /dev/null << 'EOF'
server {
    listen 80;
    server_name lienzo.tudominio.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name lienzo.tudominio.com;

    # SSL certificates (install via certbot or provide your own)
    ssl_certificate /etc/letsencrypt/live/lienzo.tudominio.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/lienzo.tudominio.com/privkey.pem;

    root /var/www/lienzo;
    index index.html;

    # Frontend static files
    location / {
        try_files $uri $uri/ /index.html;
    }

    # API proxy
    location /api/ {
        proxy_pass http://127.0.0.1:5002;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # SignalR WebSocket support
    location /hubs/ {
        proxy_pass http://127.0.0.1:5002;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 86400;
    }

    # Uploads (classroom images)
    location /uploads/ {
        proxy_pass http://127.0.0.1:5002;
        proxy_cache_valid 200 7d;
        expires 7d;
        add_header Cache-Control "public, immutable";
    }

    # Swagger (restrict in production if needed)
    location /swagger/ {
        proxy_pass http://127.0.0.1:5002;
        # Uncomment to restrict access to internal IPs
        # allow 10.0.0.0/8;
        # allow 172.16.0.0/12;
        # allow 192.168.0.0/16;
        # deny all;
    }

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    # Gzip
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml text/javascript image/svg+xml;
    gzip_min_length 1000;
    gzip_comp_level 6;

    client_max_body_size 10M;
}
EOF

# Enable site and reload
sudo ln -sf /etc/nginx/sites-available/lienzo /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t  # Verify config
sudo systemctl reload nginx
```

### 7. SSL Certificate with Let's Encrypt (optional but recommended)

```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d lienzo.tudominio.com
```

### 8. Firewall Configuration

```bash
sudo apt install -y ufw
sudo ufw allow OpenSSH
sudo ufw allow 'Nginx Full'
sudo ufw --force enable
sudo ufw status  # Verify
```

### 9. Verify Deployment

```bash
# Check API is running
curl -s http://127.0.0.1:5002/api/auth/me

# Check frontend
curl -s -o /dev/null -w "%{http_code}" http://localhost/
# Should return 200
```

### 10. Updating the Application

```bash
cd /opt/Lienzo
git pull origin main

# Backend
cd src/Lienzo.API
dotnet publish -c Release -o /opt/Lienzo/publish
sudo systemctl restart lienzo-api

# Frontend
cd /opt/Lienzo/src/Lienzo.Web
npm install && npm run build
sudo rm -rf /var/www/lienzo && sudo cp -r dist /var/www/lienzo
sudo chown -R www-data:www-data /var/www/lienzo
```

### Troubleshooting

| Issue | Check |
|-------|-------|
| API won't start | `sudo journalctl -u lienzo-api -n 50 --no-pager` |
| DB connection error | Verify credentials in `appsettings.Production.json` and PostgreSQL status |
| Frontend blank page | Check Nginx error log: `sudo tail -f /var/log/nginx/error.log` |
| Uploads 404 | Verify `www-data` owns `/opt/Lienzo/publish/wwwroot/uploads` |
| WebSocket not working | Ensure Nginx proxy has `Upgrade` and `Connection` headers for `/hubs/` |

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
