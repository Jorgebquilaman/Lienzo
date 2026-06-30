# Lienzo — Sistema de Gestión de Aulas

## Alcance del Sistema

### Visión General

Lienzo es un sistema integral de gestión de aulas universitarias que permite la reserva de espacios, control de asistencia mediante QR, sincronización con el sistema académico (SGA), administración de llaves en Bedelía, y visualización en pantallas TV públicas. Sigue una arquitectura limpia (Clean Architecture) con backend .NET 9 y frontend React + TypeScript.

---

## Módulos y Funcionalidades

### 1. Autenticación y Usuarios

- Inicio de sesión con JWT + Refresh Tokens
- Registro de usuarios
- Recuperación de contraseña por email
- Roles: Admin, Docente, Estudiante
- CRUD de usuarios (Admin)
- Sincronización automática de docentes y estudiantes desde SGA
- Activación/desactivación de usuarios

### 2. Gestión de Edificios y Aulas

- CRUD de edificios (nombre, dirección, pisos)
- CRUD de aulas (nombre, tipo, capacidad, piso, características, imagen)
- Tipos de aula: General, Danza, Dibujo, Música, Conferencia, Laboratorio, Taller, Seminario, Auditorio, Oficina
- Características: proyector, wifi, aire acondicionado, computadoras, pizarrón, sillas
- Verificación de disponibilidad en tiempo real
- Mapa interactivo del campus con estado de aulas (disponible/ocupada/mantenimiento)
- Imagen del aula (JPG/PNG/WebP, máx 5MB)
- Sincronización con SGA

### 3. Reservas

- Creación de reservas con selección de aula, fecha y horario
- Flujo de estado: Pendiente → Aprobada / Rechazada / Cancelada
- Aprobación/rechazo por administrador
- Reservas periódicas (semanal, con fecha fin)
- Cancelación de reserva individual
- Cancelación de reserva actual + todas las futuras del mismo grupo periódico
- Reasignación de aula para reservas aprobadas
- Vista de horario semanal con cuadrícula (modo "Por aulas" y "Por horarios")
- Filtros: edificio, aula, rango de fechas
- Exportación a Excel y PDF
- Recordatorios (24h y 30min antes)

### 4. Asistencia (Check-in QR)

- **Check-in del docente**: inicia una clase desde una reserva, generando un QR
- **Auto-marcado del alumno**: escanea el QR y marca su presencia
- **Marcado manual**: el docente puede marcar/desmarcar alumnos individualmente
- **Ordenamiento**: por nombre, presentes/ausentes
- **Exportación PDF** del listado de asistencia
- **Cierre de clase** por el docente
- **Sincronización bidireccional con SGA**: las asistencias se registran en la tabla `negocio.sga_clases_asistencia`

### 5. Bedelía (Control de Llaves)

- Registro de entrega de llaves a docentes
- Registro de devolución
- Transferencia de llaves entre personas
- Seguimiento de accesorios entregados junto con las llaves
- CRUD de accesorios
- Mapa de Bedelía con estado actual de cada aula y próxima reserva

### 6. TV Dashboard

- Visualización de pantalla completa para TV públicos
- Estado en tiempo real de aulas por edificio
- Códigos de colores: disponible/ocupada/mantenimiento
- Tema claro/oscuro (persistido en localStorage)
- Auto-refresh cada 15s
- Detección de orientación portrait/landscape
- Grid responsivo (4 columnas en landscape)
- Barra de progreso en footer (slide cada 15s)

### 7. Anuncios y Notificaciones

- Creación de anuncios por tipo: Cancelación, Postergación, General, Emergencia
- Destinatarios: Todos, Todos los estudiantes, Todos los docentes, Aula específica, Estudiantes específicos
- Notificaciones en tiempo real vía SignalR
- Marcado de leído / no leído

### 8. Encuestas de Aulas

- Calificación post-uso: Condición, Equipo, Limpieza, General (1-5 estrellas)
- Comentario opcional
- Vista de resumen por aula con promedios
- Analítica: KPIs, distribución, ranking top/bottom 5
- Exportación PDF

### 9. Mantenimiento

- Bloqueo de aulas por mantenimiento con fecha/hora y motivo
- Cancelación automática de reservas solapadas
- Finalización anticipada del bloqueo

### 10. Reportes y Analítica

- Reporte de uso: reservas, horas, índice de cancelación por aula
- Métricas de demanda: horas pico, demanda por tipo de aula
- Uso por propuesta académica / docente
- Carga horaria docente
- Timeline de uso de aulas
- Heatmap de ocupación
- Dashboard con KPIs: total aulas, reservas hoy, pendientes de aprobación, tasa de ocupación

### 11. Sincronización SGA

- **Edificios** desde `negocio.sga_edificaciones`
- **Aulas** desde `negocio.sga_espacios`
- **Carreras** desde `negocio.sga_propuestas`
- **Períodos** desde `negocio.sga_periodos`
- **Tipos de período** desde `negocio.sga_periodos_genericos`
- **Actividades** desde `negocio.sga_comisiones`, `sga_comisiones_bh`, `sga_asignaciones`
- **Docentes** desde actividades SGA (crea usuarios Identity)
- **Estudiantes** desde `negocio.sga_alumnos` + `mdp_personas` (crea usuarios Identity)
- **Asistencia**: lectura y escritura en `negocio.sga_clases_asistencia`

---

## Tipos de Usuario y Permisos

| Funcionalidad | Admin | Docente | Estudiante |
|---|---|---|---|
| **Autenticación** | | | |
| Iniciar sesión, registrarse, recuperar contraseña | ✓ | ✓ | ✓ |
| Ver perfil propio | ✓ | ✓ | ✓ |
| **Aulas** | | | |
| Listar y ver detalle | ✓ | ✓ | ✓ |
| Ver disponibilidad | ✓ | ✓ | ✓ |
| Crear, editar, eliminar | ✓ | | |
| Sincronizar desde SGA | ✓ | | |
| Subir imagen | ✓ | | |
| **Edificios** | | | |
| Listar | ✓ | ✓ | ✓ |
| Crear, editar, eliminar | ✓ | | |
| Sincronizar desde SGA | ✓ | | |
| **Reservas** | | | |
| Crear | ✓ | ✓ | |
| Ver todas | ✓ | Solo propias | Solo propias |
| Aprobar / Rechazar | ✓ | | |
| Reasignar aula | ✓ | | |
| Cancelar (cualquiera) | ✓ | | |
| Cancelar propias | ✓ | ✓ | ✓ |
| Cancelar futuras de periódica | ✓ | ✓ | |
| **Asistencia** | | | |
| Hacer check-in (abrir clase) | ✓ | ✓ | |
| Marcar asistencia de alumnos | ✓ | ✓ | |
| Auto-marcado vía QR | ✓ | ✓ | ✓ |
| Cerrar clase | ✓ | ✓ | |
| Sincronizar con SGA | ✓ | ✓ | |
| Exportar PDF | ✓ | ✓ | |
| **Bedelía** | | | |
| Entregar / devolver / transferir llaves | ✓ | | |
| Gestionar accesorios | ✓ | | |
| **TV Dashboard** | | | |
| Ver pantalla TV | ✓ | ✓ | ✓ |
| **Anuncios** | | | |
| Crear | ✓ | ✓ | |
| Ver | ✓ | ✓ | ✓ |
| **Encuestas** | | | |
| Crear (calificar aula) | ✓ | ✓ | ✓ |
| Ver todas y analítica | ✓ | | |
| **Mantenimiento** | | | |
| Bloquear / desbloquear aulas | ✓ | | |
| **Reportes** | | | |
| Todos los reportes | ✓ | | |
| **Configuración** | | | |
| URL pública para QR | ✓ | | |
| **Gestión Académica** | | | |
| CRUD períodos, carreras, actividades | ✓ | | |
| Sincronizar desde SGA | ✓ | | |
| **Usuarios** | | | |
| CRUD usuarios | ✓ | | |
| Sincronizar docentes/estudiantes | ✓ | | |

---

## Tecnologías

| Capa | Tecnología |
|---|---|
| Backend | .NET 9 / ASP.NET Core |
| Arquitectura | Clean Architecture (Domain, Application, Infrastructure, API) |
| Patrón | CQRS con MediatR |
| ORM | Entity Framework Core 9 + Npgsql |
| Base de datos | PostgreSQL 16 |
| Autenticación | ASP.NET Core Identity + JWT Bearer |
| Tiempo real | SignalR |
| Frontend | React 18 + TypeScript + Vite |
| UI | TailwindCSS + shadcn/ui + Lucide icons |
| Estado | TanStack Query + Zustand |
| Enrutamiento | React Router v6 |
| Validación | Zod + React Hook Form |
| Reportes | jsPDF, xlsx |
| Contenedor | Docker |
