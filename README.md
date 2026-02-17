# AppIt

AppIt includes:
- `AppIt.Web` (Angular frontend)
- `AppIt.Api` (.NET API)
- `AppIt.Data` + SQL Server (EF Core migrations and seed data)

## Docker Environments

This repository now includes a full Docker setup for running the system locally with isolated environments.

### Files Added for Docker
- `docker-compose.yml`
- `.env.docker`
- `.dockerignore`
- `AppIt.Api/Dockerfile`
- `AppIt.Api/appsettings.Docker.json`
- `AppIt.Web/Dockerfile`
- `AppIt.Web/nginx/default.conf`

### Prerequisites
- Docker Desktop (with Compose v2)

### 1. Create local Docker env file
PowerShell:

```powershell
Copy-Item .env.docker .env
```

Update `.env` values as needed, especially `MSSQL_SA_PASSWORD`.

### 2. Build and start all services

```powershell
docker compose up --build -d
```

### 3. Open the app
- Web: `http://localhost:4200`
- API: `http://localhost:5175`

### 4. Stop services

```powershell
docker compose down
```

To also remove the SQL volume:

```powershell
docker compose down -v
```

## Environment Variables (Docker)

Main values from `.env`:
- `MSSQL_SA_PASSWORD`
- `APPIT_DB_NAME`
- `APPIT_SQL_PORT`
- `APPIT_API_PORT`
- `APPIT_WEB_PORT`
- `APPIT_ADMIN_PASSWORD`
- `STRIPE_SECRET_KEY`
- `PAYPAL_CLIENT_ID`
- `PAYPAL_CLIENT_SECRET`
- `PAYPAL_ENVIRONMENT`

## Notes

- API runs with `ASPNETCORE_ENVIRONMENT=Docker`.
- API DB connection is injected by Docker using `ConnectionStrings__DefaultConnection`.
- API applies migrations and seeds data automatically on startup.
- Frontend serves via nginx and proxies `/api/*` to the API container.

## Default Seeded Admin
- Email: `admin@appit.com`
- Password: value from `APPIT_ADMIN_PASSWORD` (default `Admin@2026`)
