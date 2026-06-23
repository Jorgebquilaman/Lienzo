# Actualización del Sistema Lienzo

Pasos para actualizar la aplicación desde GitHub en el servidor.

## 1. Backend (API)

```bash
# Ir al directorio de publish
cd /opt/Lienzo/publish

# Detener el servicio
systemctl stop lienzo-api

# Ir al repositorio
cd /opt/Lienzo

# Pull de últimos cambios
git pull origin main

# Publicar backend
dotnet publish src/Lienzo.API/Lienzo.API.csproj -c Release -o /opt/Lienzo/publish

# Aplicar migraciones a la base de datos (si las hay)
# Revisar si hay nuevas migraciones:
ls src/Lienzo.Infrastructure/Data/Migrations/

# Si hay migraciones nuevas, generar el SQL y aplicarlo:
dotnet ef migrations script --output /tmp/migrate.sql --project src/Lienzo.Infrastructure --startup-project src/Lienzo.API
docker exec -i lienzo-db psql -U lienzo -d lienzo -f /tmp/migrate.sql

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

# O si se usa la publish del backend para servir los archivos estáticos:
cp -r dist/* /opt/Lienzo/publish/wwwroot/
```

## 3. Verificar

- API: `curl http://localhost:5002/api/health` (o el endpoint configurado)
- Web: `curl http://localhost` (o la URL del dominio)
- Base de datos: `docker exec -i lienzo-db psql -U lienzo -d lienzo -c "SELECT version();"`

## 4. Rollback

Si algo falla:

```bash
# Backend: volver al commit anterior
cd /opt/Lienzo
git reset --hard HEAD~1
dotnet publish src/Lienzo.API/Lienzo.API.csproj -c Release -o /opt/Lienzo/publish
systemctl restart lienzo-api

# Frontend: rebuild con versión anterior
cd /opt/Lienzo/src/Lienzo.Web
git reset --hard HEAD~1
npm run build
cp -r dist/* /var/www/lienzo/html/
```
