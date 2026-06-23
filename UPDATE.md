# Actualización del Sistema Lienzo

Pasos para actualizar la aplicación desde GitHub en el servidor.

## 1. Backend (API)

```bash
# Ir al repositorio
cd /opt/Lienzo

# Pull de últimos cambios
git pull origin main

# Detener el servicio
systemctl stop lienzo-api

# Publicar backend
dotnet publish src/Lienzo.API/Lienzo.API.csproj -c Release -o /opt/Lienzo/publish

# Aplicar migraciones a la base de datos
```

### Migraciones de Base de Datos

La base de datos es PostgreSQL. Las migraciones se aplican con SQL directo porque la connection string de `LienzoDbContextFactory` usa credenciales distintas a la BD real.

#### Opción 1: PostgreSQL local
```bash
# Generar script SQL con las migraciones nuevas
dotnet ef migrations script --output /tmp/migrate.sql \
  --project src/Lienzo.Infrastructure \
  --startup-project src/Lienzo.API

# Aplicar a la BD local
psql -h localhost -U lienzo -d lienzo -f /tmp/migrate.sql
```

#### Opción 2: PostgreSQL en Docker
```bash
# Generar script SQL
dotnet ef migrations script --output /tmp/migrate.sql \
  --project src/Lienzo.Infrastructure \
  --startup-project src/Lienzo.API

# Aplicar al contenedor
docker exec -i lienzo-db psql -U lienzo -d lienzo -f /tmp/migrate.sql
```

#### Opción 3: PostgreSQL remoto
```bash
# Generar script SQL
dotnet ef migrations script --output /tmp/migrate.sql \
  --project src/Lienzo.Infrastructure \
  --startup-project src/Lienzo.API

# Aplicar a la BD remota (reemplazar host, usuario y puerto según corresponda)
psql -h 192.168.1.100 -U lienzo -d lienzo -f /tmp/migrate.sql
```

> **Nota:** Si no hay migraciones nuevas, el comando `dotnet ef migrations script` mostrará un mensaje indicando que no hay nada que aplicar. Se puede omitir este paso.

> **Nota 2:** La contraseña de la BD es `lienzo123`. Si `psql` la pide, se puede usar `PGPASSWORD=lienzo123 psql ...` para evitar el prompt.

```bash
# Iniciar el servicio
systemctl start lienzo-api

# Ver logs
journalctl -u lienzo-api -f
```

## 2. Frontend (Web)

```bash
cd /opt/Lienzo/src/Lienzo.Web

# Pull de últimos cambios
git pull origin main

# Instalar dependencias (si cambiaron)
npm ci

# Build
npm run build

# Copiar build al directorio servido por nginx
cp -r dist/* /var/www/lienzo/html/

# O si se sirve estático desde el propio backend:
cp -r dist/* /opt/Lienzo/publish/wwwroot/
```

## 3. Verificar

```bash
# API
curl http://localhost:5002/api/health

# Web
curl http://localhost

# Base de datos
# Docker:
docker exec -i lienzo-db psql -U lienzo -d lienzo -c "SELECT version();"
# Local:
psql -h localhost -U lienzo -d lienzo -c "SELECT version();"
```

## 4. Rollback

Si algo falla:

```bash
# Volver al commit anterior
cd /opt/Lienzo
git reset --hard HEAD~1

# Rebuild backend
dotnet publish src/Lienzo.API/Lienzo.API.csproj -c Release -o /opt/Lienzo/publish
systemctl restart lienzo-api

# Rebuild frontend
cd /opt/Lienzo/src/Lienzo.Web
npm run build
cp -r dist/* /var/www/lienzo/html/
```

> **Importante:** El rollback de base de datos requiere revertir las migraciones manualmente si se aplicaron cambios de esquema. No hay un comando automático para esto.
