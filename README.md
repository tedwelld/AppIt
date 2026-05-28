# AppIt

AppIt includes:
- `AppIt.Web` (Angular frontend)
- `AppIt.Api` (.NET API)
- `AppIt.Data` + SQL Server (EF Core migrations and seed data)

## Docker Desktop Setup

This repository already includes the Docker files needed to run the full stack locally:
- `docker-compose.yml`
- `.env.docker`
- `.dockerignore`
- `AppIt.Api/Dockerfile`
- `AppIt.Api/appsettings.Docker.json`
- `AppIt.Web/Dockerfile`
- `AppIt.Web/nginx/default.conf`

### Prerequisites

- Docker Desktop
- Docker Compose v2 (`docker compose version`)
- Docker Desktop set to Linux containers

## Quick Start

Run these commands from the repository root:

```powershell
Copy-Item .env.docker .env
docker compose build
docker compose up -d
docker compose ps
```

If you do not create `.env`, `docker-compose.yml` still has working local defaults. Copying `.env.docker` to `.env` is recommended when you want to change passwords, ports, or payment provider values.

## Default Docker URLs

- Web UI: `http://localhost:4200`
- API root: `http://localhost:5175`
- Swagger UI: `http://localhost:5175/swagger`
- SQL Server from host tools: `localhost,1433`

## Containers Created

- `appit-sqlserver`
- `appit-api`
- `appit-web`

## Exact Docker Commands

### Build the images

```powershell
docker compose build
```

### Start the full system in the background

```powershell
docker compose up -d
```

### Build and start in one command

```powershell
docker compose up -d --build
```

### Check container status

```powershell
docker compose ps
```

### Follow logs

```powershell
docker compose logs -f sqlserver
docker compose logs -f api
docker compose logs -f web
```

### Restart a specific container

```powershell
docker compose restart api
docker compose restart web
docker compose restart sqlserver
```

### Rebuild only the API or web image

```powershell
docker compose up -d --build api
docker compose up -d --build web
```

### Stop the running containers

```powershell
docker compose down
```

### Stop containers and remove the SQL volume

```powershell
docker compose down -v
```

### Remove old images that were built for this stack

```powershell
docker image prune -f
```

## Environment Variables Used by Docker

Main values you can override in `.env`:
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

## Runtime Notes

- API runs with `ASPNETCORE_ENVIRONMENT=Docker`.
- API DB connection is injected through `ConnectionStrings__DefaultConnection`.
- API applies migrations and seeds data automatically on startup.
- Frontend is served by Nginx and proxies `/api/*` to the API container.
- SQL data is persisted in the `appit_sql_data` Docker volume.

## Default Seeded Admin

- Email: `admin@appit.com`
- Password: value from `APPIT_ADMIN_PASSWORD` (default `Admin@2026`)
