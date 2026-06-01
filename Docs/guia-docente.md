# Guía del Docente — Lienzo

## 1. Inicio de Sesión

- Ir a `/login`
- Ingresar email y contraseña
- Si no tienes cuenta, contacta al administrador o usa **Registrarse** (`/register`)

> Si olvidaste tu contraseña, usa **¿Olvidaste tu contraseña?** en la pantalla de login. Recibirás un código en tu email para restablecerla.

---

## 2. Dashboard

Al iniciar sesión (`/`) verás:

- **Tus próximas reservas** del día
- **Acciones rápidas**: Enviar anuncio, Explorar aulas
- **Anuncios recientes** del sistema

---

## 3. Explorar Aulas

`/classrooms` → Buscador y mapa del campus

### Vista Lista
- Tarjetas con imagen, nombre, edificio, piso, tipo y capacidad
- Filtros: búsqueda por nombre, edificio, tipo de aula, ordenar por nombre o capacidad

### Vista Mapa
- Mapa interactivo del campus con códigos de colores:
  - **Verde**: aula disponible
  - **Rojo**: aula ocupada
  - **Amarillo**: aula en mantenimiento

### Detalle del Aula (`/classrooms/:id`)
- Información completa: imagen, nombre, tipo, edificio, piso, capacidad, descripción
- **Características**: proyector, WiFi, AC, computadoras, pizarrón, sillas
- Gráfico de ocupación semanal
- **Horario semanal** grid con reservas existentes
- **Reservar ahora** (solo si el aula no está en mantenimiento)

---

## 4. Reservaciones

`/reservations` → Mis reservas organizadas en 4 pestañas

### Pestañas
| Pestaña | Qué muestra |
|---------|-------------|
| Próximas | Reservas aprobadas que aún no ocurrieron |
| Pendientes | Reservas esperando aprobación del administrador |
| Pasadas | Reservas ya completadas (se pueden calificar) |
| Canceladas | Reservas canceladas |

### Crear una reserva
1. Ir al detalle del aula (`/classrooms/:id`) o desde el horario
2. Hacer clic en **Reservar ahora**
3. Completar: título, fecha, hora inicio, hora fin
4. Opcional: enlazar a una actividad académica, marcar como **semanal** (elegir días y fecha fin)
5. El sistema verifica disponibilidad en tiempo real
6. Enviar → queda **Pendiente** hasta que un administrador la apruebe

### Editar / Cancelar
- Desde la pestaña **Próximas** o **Pendientes**
- Botón **Editar** o **Cancelar**
- Una vez aprobada, solo puedes cancelarla (no editar)

### Calificar aula (Encuesta)
- En la pestaña **Pasadas**, las reservas completadas tienen botón **Calificar**
- Puntuar: Condición, Equipo, Limpieza, Calificación General (1-5 estrellas)
- Comentario opcional

---

## 5. Horario General

`/schedule` → Vista de horario día por día

### Cómo usar
1. Seleccionar **edificio** (obligatorio)
2. Opcional: seleccionar **aula** específica
3. Navegar entre días con las flechas o el selector de fecha
4. Botón **Hoy** para volver al día actual

### Colores de reservas
- **Verde**: Aprobada
- **Amarillo**: Pendiente
- **Azul**: Completada

### Alternar modo de color
- Puedes cambiar a color por **periodo académico** para ver a qué periodo pertenece cada reserva

### Ver detalle
- Haz clic en cualquier barra de reserva → modal con toda la información (incluye datos de la actividad académica)

### Reglas de horario
- Sábado: solo hasta las 16:00
- Domingo y feriados: bloqueados (no hay reservas)

---

## 6. Anuncios

`/announcements` → Los docentes pueden **enviar** y **ver** anuncios

### Redactar anuncio
1. Pestaña **Redactar**
2. Título, cuerpo del mensaje
3. **Tipo**: General, Cancelación, Postergación, Emergencia
4. **Destinatarios**: Todos los estudiantes, Todos los docentes, Todos
5. Enviar

### Anuncios enviados
- Pestaña **Enviados** → historial de anuncios que has creado

---

## 7. Mis Encuestas

`/surveys` → Tus evaluaciones de aulas

### Pendientes
- Lista de reservas pasadas aprobadas que **aún no has calificado**
- Botón **Calificar** → modal con estrellas (Condición, Equipo, Limpieza, General) + comentario

### Completadas
- Historial de todas tus evaluaciones anteriores
- Muestra las calificaciones que diste en cada categoría

---

## 8. Perfil

`/profile` → Editar tu información personal

- Cambiar nombre, apellido, foto de avatar
- **Cambiar contraseña**: requiere contraseña actual + nueva + confirmación

---

## 9. Notificaciones

- Campana en la esquina superior derecha → panel de notificaciones
- Recibirás notificaciones cuando:
  - Tu reserva sea aprobada o rechazada
  - Una reserva tuya sea cancelada por mantenimiento
  - Recibas un anuncio

---

## 10. Navegación

En la parte superior (escritorio) o barra inferior (móvil):

| Ícono | Sección |
|-------|---------|
| Dashboard | `/` |
| Aulas | `/classrooms` |
| Reservaciones | `/reservations` |
| Horario | `/schedule` |
| Anuncios | `/announcements` |
| Mis Encuestas | `/surveys` |

Tu rol y nombre aparecen en la esquina superior derecha → clic para ir a Perfil o Cerrar sesión.
